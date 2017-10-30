/// Implementação (incorreta) do sincronizador ManualResetEvent 
/// com fast path lock-free, isto é , o teste preliminar na operação de wait
/// é feito fora do monitor, bem como o teste preliminar na operração set
/// 
/// Jorge Martins, outubro de 2017
/// 

using System;
using System.Threading;
using Utils;

namespace Aula_2017_10_26.LockFree {
    public class ManualResetEvent {
        private Object monitor = new Object();  // the monitor
        private volatile bool signaled;         // the event state
        private volatile int waiters;           // number of waiter threads
        private int signalVersion;              // number of signal operations

        public ManualResetEvent(bool initialState) {
            signaled = initialState;
        }

        public bool await(int timeout) { // throws InterruptedException
            // (really) fast path (lock-free)
            if (signaled) return true;
            if (timeout == 0) return false;
            lock(monitor) {
                // prepare wait
                int currentVersion = signalVersion;

                waiters++;
                // fast (non blocking)  path
                
                // !!!
                
                if (signaled) {
                    waiters--;
                    return true;
                }
                try {
                    do {

                        int refTime = Environment.TickCount;

                        Monitor.Wait(monitor, timeout);
                        if (currentVersion != signalVersion)
                            return true;

                        timeout = SynchUtils.RemainingTimeout(refTime, timeout);
                        if (timeout == 0)
                            return false;   // abort operation on timeout

                    }
                    while (true);
                }
                finally {
                    waiters--;
                }    
            }
        }

        public void set() {

            if (!signaled) {

                signaled = true;

                // !!!
                
                if (waiters > 0) {
                    lock(monitor) {
                        if (waiters > 0) {
                            signalVersion++;
                            Monitor.PulseAll(monitor);
                        }
                    }
                }
            }
        }

        public void reset() {
            lock(monitor) {
                signaled = false;
            }
        }

        public int getWaiters() {
            lock(monitor) {
                return waiters;
            }
        }
    }

}
