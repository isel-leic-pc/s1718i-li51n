package exercicios1;

import java.util.LinkedList;
import java.util.List;

import static utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;

/**
 * Created by jmartins on 01/10/2017.
 */
public class AutoResetEvt {
    private Object monitor = new Object();
    private boolean signaled; // event state

    // auxiliary class maintaining waiter state
    private static class Waiter {
        boolean done;
    }

    private List<Waiter> waiters; // waiters list

    public AutoResetEvt(boolean initialState) {
        signaled = initialState;
        waiters = new LinkedList<Waiter>();
    }

    public void signal() {
        synchronized(monitor) {
            if (waiters.size() > 0) {
                Waiter w = waiters.remove(0);
                w.done = true;
                monitor.notifyAll();
            }
            else {
                signaled= true;
            }
        }
    }

    public void pulseAll() {
        synchronized(monitor) {
            for(Waiter w : waiters)
                w.done = true;
            waiters.clear();
            monitor.notifyAll();
        }
    }

    public boolean await(long timeout) throws InterruptedException {
        synchronized(monitor) {
            // fast path
            // note that if it is signaled, the list must be empty
            if (signaled) {
                signaled = false;
                return true;
            }
            if (timeout == 0) {
                return false;
            }
            // prepare wait
            Waiter w = new Waiter();
            waiters.add(w);
            do {
                try {
                    long refTime = System.currentTimeMillis();
                    if (timeout == INFINITE)
                        monitor.wait();
                    else
                        monitor.wait(timeout);
                    if (w.done) {
                        return true;
                    }
                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0)  {
                        // abort wait
                        waiters.remove(w);
                        return false;
                    }

                }
                catch(InterruptedException e) {
                    if (w.done) {
                        // delay interruption and return success
                        Thread.currentThread().interrupt();
                        return true;
                    }
                    // abort wait
                    waiters.remove(w);
                    throw e;
                }
            } while (true);
        }
    }
}
