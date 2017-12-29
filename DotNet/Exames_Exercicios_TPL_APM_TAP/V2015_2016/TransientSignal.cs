using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace V2015_2016
{
    class TransientSignal
    {
        private LinkedList<bool> waiters;
        private object monitor = new object();

        private int adjustTimeout(int refTime, int timeout) {
            return 0;
        }
        public TransientSignal() {
            waiters = new LinkedList<bool>();
        }
        public bool await(int timeout) { //throws InterruptedException;

            lock (monitor) {
                var node = waiters.AddLast(false);

                do {
                    try {
                        int refTime = Environment.TickCount;
                        Monitor.Wait(monitor, timeout);
                        if (node.Value) return true;
                        timeout = adjustTimeout(refTime, timeout);
                        if (timeout <= 0) {
                            waiters.Remove(node);
                            return false;
                        }
                    }
                    catch (ThreadInterruptedException) {
                        if (node.Value) {
                            Thread.CurrentThread.Interrupt();
                            return true;
                        }
                        waiters.Remove(node);
                        throw;
                    }
                }
                while (true);
            }
            
        }

        public void signal() {
            lock (monitor) {
                if (waiters.Count == 0) return;
                var node = waiters.First;
                waiters.RemoveFirst();
                node.Value = true;
                Monitor.PulseAll(monitor);
            }
        }

        public void signalAll() {
             lock (monitor) {
                 if (waiters.Count == 0) return;
               
                 var node = waiters.First;
                 while(node != null) {
                     node.Value=true;
                     node = node.Next;
                 }
                 waiters.Clear();
                 Monitor.PulseAll(monitor);
            }
        }
    }
}
