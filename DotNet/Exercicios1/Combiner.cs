using System;
using System.Collections.Generic;
using System.Threading;
using Utils;

namespace Exercicios1 {
    public class Combiner<L, R> where L : class
                                where R : class {

        private object monitor = new object();      // the monitor object

        private LinkedList<Offer> lOffers;          // pending L offers
        private LinkedList<Offer> rOffers;          // pending R offers
        private LinkedList<Pair> pairs;             // pairs queue
        private LinkedList<object> waiters;         // waiters queue
    

        private class Offer {
            private L offerL;
            private R offerR;

            public Offer(L lval) { offerL = lval;  }
            public Offer(R rval) { offerR = rval;  }

            public L OfferL{
                get { return offerL; }
            }

            public R OfferR {
                get { return offerR; }
            }
        }

      

        public Combiner() {
            pairs = new LinkedList<Pair>();
            waiters = new LinkedList<object>();
            rOffers = new LinkedList<Offer>();
            lOffers = new LinkedList<Offer>();
        }


        public class Pair {
            public readonly L left;
            public readonly R right;

            internal Pair(L left, R right) {
                this.left = left; this.right = right;
            }
            
           
        }

        public void PutLeft(L left) {
            lock (monitor) {
                // check if the there are rOffers!
                if (rOffers.Count > 0) {
                    Offer o = rOffers.First.Value;
                    rOffers.RemoveFirst();
                    pairs.AddLast(new Pair(left, o.OfferR));
                    if (waiters.Count > 0) {
                        Monitor.PulseAll(monitor);  // the front requested pair is completed!
                    }         
                }
                else {
                    lOffers.AddLast(new Offer(left));
                }
            }
        }

        public void PutRight(R right) {
            lock (monitor) {
                // check if the there are lOffers!
                if (lOffers.Count > 0) {
                    Offer o = lOffers.First.Value;
                    lOffers.RemoveFirst();
                    pairs.AddLast(new Pair(o.OfferL, right));
                    if (waiters.Count > 0) {
                        Monitor.PulseAll(monitor);  // the front requested pair is completed!
                    }
                }
                else {
                    rOffers.AddLast(new Offer(right));
                }
            }
        }

        public Pair Take(int timeout) // throws ThreadInterruptedException
        {
            lock (monitor) {
                // fast path
                if (waiters.Count > 0 /* avoid barging */ && pairs.Count > 0) {
                    var pair = pairs.First.Value;
                    pairs.RemoveFirst();
                    return pair;
                }
                if (timeout == 0) {
                    return null;
                }
                // prepare wait
                var req = waiters.AddLast((object)null);
                do {
                    try {
                        int refTime = Environment.TickCount;
                        Monitor.Wait(monitor, timeout);
                        if (waiters.First == req && pairs.Count > 0) {
                            var pair = pairs.First.Value;
                            pairs.RemoveFirst();
                            return pair;
                        }
                        timeout = SynchUtils.RemainingTimeout(refTime, timeout);
                        if (timeout == 0) {
                            // abort wait
                            waiters.Remove(req);
                            return null;
                        }
                    }
                    catch (ThreadInterruptedException) {
                        // abort wait
                        waiters.Remove(req);
                        throw;
                    }
                }
                while (true);
            }
        }
    }
}

