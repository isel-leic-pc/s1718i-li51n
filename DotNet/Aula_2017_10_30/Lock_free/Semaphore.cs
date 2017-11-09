using System;

using System.Threading;
using Utils;

namespace Aula_2017_10_30.Lock_free {
    public class Semaphore {

        private Object monitor = new Object();
        
        private volatile int permits; // semaphore units 
        private volatile int waiters; // acquire waiters count
        public Semaphore(int initial) {
            permits = initial;
        }

        private bool TryAcquire() {
            int p;
            do {
                p = permits;
                if (p == 0)
                    return false; // no permits avaiable, just return false
            }
            while (Interlocked.CompareExchange(
                    ref permits,
                    p - 1, 
                    p) != p);
            return true;
        }


        public bool  Acquire(int timeout) { // throws InterruptedException 
            // lock-free fast path 
            if (TryAcquire())
                return true;
            if (timeout == 0)  
                return false;
             
            lock (monitor) {
              
                while (true) {
                    waiters++;
                    Thread.MemoryBarrier();
                    // There is a need for a barrier between  waiters increment
                    // and permites read in TryAquire!
                    // And none exists :( ...
                    if ( TryAcquire()) {
                        waiters--;
                        return true;
                    }
                    try {
                        int refTime = Environment.TickCount;
                        Monitor.Wait(monitor, timeout);
                        if (TryAcquire())  
                            return true;
                         
                        timeout = SynchUtils.RemainingTimeout(refTime, timeout);
                        if (timeout == 0)  
                            return false;
                    }
                    catch (ThreadInterruptedException e) {
                        if (permits > 0) Monitor.Pulse(monitor);
                        throw e;
                    }
                    finally {
                        waiters--;
                    }

                }
            }
        }
        public void Acquire() { // throws InterruptedException 
            Acquire(Timeout.Infinite);
        }

        public void Release() {
            Interlocked.Increment( ref permits);
            // There is a need for a barrier between  waiters increment
            // and subsequent waiters read  
            // And the previous increment serves as just that!
            if (waiters > 0) {
                lock (monitor) {
                    if (waiters > 0)
                        Monitor.Pulse(monitor);
                }
            }
        }

    }
}
