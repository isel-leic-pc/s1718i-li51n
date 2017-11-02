/**
 * Implementation of manual reset event synchronizer
 * with lock-free fast path
 *
 * Jmartins, october 2017
 */
package aula_2017_10_26;

import static utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;

public class ManualResetEvent {
    private Object monitor = new Object();      // the monitor
    private  boolean signaled;       // the event state
    private  int waiters;            // threads waiting for sinalization
    private int signalVersion;                  // to support batch notification

    public ManualResetEvent(boolean initialState) {
        signaled = initialState;
    }

    public boolean await(long timeout) throws InterruptedException {
        // lock-free fast path
        if (signaled) return true;
        if (timeout == 0) return false;

        synchronized(monitor) {
            int currentVersion = signalVersion;
            do {
                try {
                    // prepare wait
                    waiters++;

                    if (signaled) return true; // last chance!

                    long refTime = System.currentTimeMillis();
                    if (timeout == INFINITE)
                       monitor.wait();
                    else
                       monitor.wait(timeout);
                    if (currentVersion != signalVersion)
                        return true;
                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0) {
                       // abort operation on timeout
                       return false;
                    }
                }
                finally {
                   waiters--;
                }
            }
            while(true);
        }
    }

    public void await() throws InterruptedException {
        await(INFINITE);
    }

    public void set()  {
        signaled = true;
        if (waiters > 0) {
            synchronized (monitor) {
                signalVersion++;
                monitor.notifyAll();
            }
        }
    }

    public void reset()  {
        synchronized (monitor) {
            signaled = false;
        }
    }

    public int getWaiters() {
        synchronized (monitor) {
            return waiters;
        }
    }
}
