using System;
using System.Collections.Generic;
using System.Threading;
using Utils;

namespace Exercicios1 {
    public class Combiner<L, R> where L : class
                                where R : class {

        private object monitor = new object();  // the monitor object
        private LinkedList<Pair> pairs;         // pairs queue
        private LinkedList<object> waiters;     // waiters queue

        public Combiner() {
            pairs = new LinkedList<Pair>();
            waiters = new LinkedList<object>();
        }


        public class Pair {
            private L left;
            private R right;

            internal Pair(L left) { this.left = left; }
            internal Pair(R right) { this.right = right; }

            internal bool IsCompleted {
                get { return left != null && right != null; }
            }

            public R Right {
                get { return right; }
                internal set { right = value;  }
            }

            public  L Left {
                get { return left;  }
                internal set { left = value; }
            }
        }

        public void PutLeft(L left) {
            lock (monitor) {

                // check if a pair is completed!
                if (pairs.Count > 0) {
                    var p = pairs.First.Value;
                    if (p.Left == null) {
                        p.Left = left;
                        Monitor.PulseAll(monitor); // it is, indeed!
                        return;
                    }
                }

                // insert a new pair on the list
                var pair = new Pair(left);
                pairs.AddLast(pair);

            }
        }
        public void PutRight(R right) {
            lock (monitor) {
                // check if a pair is completed!
                if (pairs.Count > 0) {
                    var p = pairs.First.Value;
                    if (p.Right == null) {
                        p.Right = right;
                        Monitor.PulseAll(monitor); // it is, indeed!
                        return;
                    }
                }

                // insert a new pair on the list
                var pair = new Pair(right);
                pairs.AddLast(pair);
            }
        }

        private bool PairCompleted() {
            return pairs.Count > 0 && pairs.First.Value.IsCompleted;
        }
        public Pair Take(int timeout) // throws ThreadInterruptedException
        {
            lock (monitor) {
                // fast path
                if (waiters.Count > 0 /* avoid barging */ && PairCompleted()) {
                    var pair = pairs.First.Value;
                    pairs.RemoveFirst();
                    return pair;
                }
                if (timeout == 0) {
                    return null;
                }
                // prepare wait
                var req = waiters.AddLast((object) null);
                do {
                    try {
                        int refTime = Environment.TickCount;
                        Monitor.Wait(monitor, timeout);
                        if (waiters.First == req && PairCompleted()) {
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
