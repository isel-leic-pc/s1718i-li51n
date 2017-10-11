using System;
using System.Collections.Generic;

using System.Threading;
using Utils;

namespace Exercicios1 {
    public class AutoResetEvt {
        private Object monitor = new Object();
        private bool signaled; // event state
 
        private LinkedList<bool> waiters; // waiters list

        public AutoResetEvt(bool initialState) {
            signaled = initialState;
            waiters = new LinkedList<bool>();
        }

        public void Signal() {
            lock(monitor) {
                if (waiters.Count > 0) {
                    var wNode = waiters.First;
                   
                    waiters.RemoveFirst();
                    wNode.Value = true;  
                    Monitor.PulseAll(monitor);
                }
                else {
                    signaled = true;
                }
            }
        }

        public void PulseAll() {
            lock(monitor) {
                for (LinkedListNode<bool> it = waiters.First; it != null; it = it.Next)  
                    it.Value = true;
                waiters.Clear();
                Monitor.PulseAll(monitor);
            }
        }

        public bool  Wait(int timeout) { //  throws InterruptedException  
            lock(monitor) {
                // fast path

                // note that is signaled the list must be empty
                if (signaled) {
                    signaled = false;
                    return true;
                }
                if (timeout == 0) {
                    return false;
                }
                
                var wNode = waiters.AddLast(false);
                do {
                    try {
                        int refTime = Environment.TickCount;
                        Monitor.Wait(monitor, timeout);
                        if (wNode.Value == true) {
                            return true;
                        }
                        timeout = SynchUtils.RemainingTimeout(refTime, timeout);
                        if (timeout == 0) {
                            // abort wait
                            waiters.Remove(wNode);
                            return false;
                        }
                    }
                    catch (ThreadInterruptedException e) {
                        if (wNode.Value == true) {
                            // delay interruption and return success
                            Thread.CurrentThread.Interrupt();
                            return true;
                        }
                        // abort wait
                        waiters.Remove(wNode);
                        throw e;
                    }
                } while (true);
            }
        }
    }

}
