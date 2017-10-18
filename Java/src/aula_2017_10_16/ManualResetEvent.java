/**
 * Implementation of manual reset event synchronizer
 * In this case, execution delegation technique is mandatory
 *
 * Jmartins, october 2017
 */
package aula_2017_10_16;

import java.util.LinkedList;
import java.util.List;

import static utils.SynchUtils.remainingTimeout;

public class ManualResetEvent {
    private Object monitor = new Object();  // the monitor
    private boolean signaled;               // the event state
    private List<Request> waiters;          // waiting queue for sinalization await

    // auxiliary class to support execution delegation
    private class Request { boolean done; }

    public ManualResetEvent(boolean initialState) {
        signaled = initialState;
        waiters = new LinkedList<>();
    }


    public boolean await(long timeout) throws InterruptedException {
        synchronized(monitor) {
            // fast path
            if (signaled) return true;
            if (timeout == 0) return false;

            // prepare wait
            Request r = new Request();
            waiters.add(r);
            do {
                try {
                    long refTime = System.currentTimeMillis();
                    monitor.wait(timeout);
                    if (r.done) return true;
                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0) {
                        // abort operation on timeout
                        waiters.remove(r);
                        return false;
                    }

                }
                catch(InterruptedException e) {
                    if (r.done) {
                        Thread.currentThread().interrupt();
                        return true;
                    }
                    // abort operation on interruption
                    waiters.remove(r);
                    throw e;
                }
            }
            while(true);
        }
    }

    public void set()  {
        synchronized(monitor) {
            if (!signaled) {
                for (Request r : waiters) r.done = true;
                waiters.clear();
                signaled=true;
                monitor.notifyAll();
            }
        }
    }

    public void reset()  {
        synchronized(monitor) {
            signaled = false;
        }
    }
}
