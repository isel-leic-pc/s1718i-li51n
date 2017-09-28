package aula_2017_09_25;

import java.util.LinkedList;
import java.util.List;

import static  utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;

/**
 * Created by jmartins on 27/09/2017 (based on Carlos Martins implementation)
 * This monitor implements a counter semaphore, i.e., a semaphore with acquire and release operations
 * having arbitrary units value. Timeout on acquire is supported.
 *
 * This implementation use a FIFO discipline satisfying acquires in order to avoid thread starvation
 */
public class CounterSemaphore2 {
    private Object monitor = new Object();
    private int units; // the current sempahore units

    List<Request> requests;

    /**
     * Internal class to represent an acquire operation on the FIFO queue
     */
    private static class Request {
        int reqUnits; // request units;
        public Request(int permits) {
            reqUnits = permits;
        }
    }

    // auxiliary methods
    private Request atFront() {
        return requests.get(0);
    }

    private void notifyWaiters() {
        if (requests.size() > 0 && units >= atFront().reqUnits) {
            monitor.notifyAll();
        }
    }

    // public interface

    public CounterSemaphore2(int initial) {
        units = initial;
        requests = new LinkedList<>();
    }

    /**
     * Release is simple
     * Just increase semaphore units and notify all waiting threads (if at least one can proceed)
     * @param n the units to release
     */
    public void release(int n) {
        synchronized (monitor) {
            units += n;
            notifyWaiters();

        }
    }

    public boolean acquire(int n, long timeout) throws InterruptedException {
        synchronized (monitor) {
            // fast path to success or fail
            if (requests.size() == 0 && units > n) {
                // note the requests queue must be empty to avoid barging
                units -= n;
                return true;
            }
            if (timeout == 0) {
                return false;
            }

            // prepare wait
            Request req = new Request(n);
            requests.add(req);

            do {
                try {
                    long refTime = System.currentTimeMillis();
                    if (timeout == INFINITE)
                        monitor.wait();
                    else
                        monitor.wait(timeout);
                    if (atFront() == req && units > n) {
                        // if we are at the front of the queue and there are sufficient units all is ok!
                        requests.remove(0);
                        units -= n;
                        return true;
                    }

                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0) {
                        // end operation on timeout
                        // because queue state change a broadcast notification is needed
                        // note the remove operation is O(n). If this is a performance concern
                        // we must use a list implementation where we can access nodes
                        requests.remove(req);
                        notifyWaiters();
                        return false;
                    }

                } catch (InterruptedException e) {
                    // operation interrupted
                    // because queue state change a broadcast notification is needed
                    // note the remove operation is O(n). If this is a performance concern
                    // we must use a list implementation where we can access nodes
                    requests.remove(req);
                    notifyWaiters();
                    throw e;
                }

            }
            while(true);
        }
    }

    /**
     * Overloaded Acquire without timeout
     * @param n units to acquire
     * @throws InterruptedException
     */
    public void acquire(int n) throws InterruptedException {
        acquire(n, INFINITE);
    }


}

