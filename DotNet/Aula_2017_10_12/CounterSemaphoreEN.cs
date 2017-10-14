/**
 * CounterSemaphore implementaion using execution delegation and explicit notification
 * JMartins, Out 2017
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Utils;

namespace Aula_2017_10_12 {
    class CounterSemaphoreEN {
        private Object monitor = new object();
        private int units;

        private class Request {
            internal int reqUnits;
            internal bool done;

            public Request(int n) {
                reqUnits = n;
            }
        }

        private LinkedList<Request> requests;

        public CounterSemaphoreEN(int initial) {
            units = initial;
            requests = new LinkedList<Request>();
        }

        public bool Acquire(int n, int timeout) {
           lock(monitor) { 
                if (requests.Count == 0 && units >= n) {
                    units -= n;
                    return true;
                }
                if (timeout == 0) {
                    return false;
                }

                Request req = new Request(n);
                LinkedListNode<Request> node = requests.AddLast(req);
                do {
                    try {
                        int refTime = Environment.TickCount;
                        monitor.Await(req, timeout);
                        if (req.done) return true;
                        timeout = SynchUtils.RemainingTimeout(
                            refTime, timeout);
                        if (timeout == 0) {
                            requests.Remove(node);
                            return false;
                        }
                            
                    }
                    catch (ThreadInterruptedException) {
                        if (req.done) {
                            Thread.CurrentThread.Interrupt();
                            return true;
                        }
                        requests.Remove(node);
                        throw;
                    }
                }
                while (true);
            }
        }

        public void Release(int n) {
            lock(monitor) {
                units += n;
                while (requests.Count > 0) {
                    Request r = requests.First.Value;
                    if (r.reqUnits > units)
                        return;
                    units -= r.reqUnits;
                    r.done = true;
                    requests.RemoveFirst();
                    monitor.Signal(r);
                } 
            }
        }

        public void Acquire(int units) {
            Acquire(units, Timeout.Infinite);
        }

        /// <summary>
        /// Auxiliary method for tests purpose
        /// </summary>
        public int WaitersCount {
            get {
                lock(monitor) {
                    return requests.Count;
                }
            }
        }

        /// <summary>
        /// Auxiliary method for tests purpose
        /// </summary>
        public int Units {
            get {
                lock (monitor) {
                    return units;
                }
            }
        }

    }
}
