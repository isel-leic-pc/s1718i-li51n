package aula_2017_09_25;

/**
 * Created by jmartins on 28/09/2017.
 */

import java.util.LinkedList;
import java.util.List;

import static utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;

/**
 * Created by jmartins on 27/09/2017
 * This monitor implements a counter semaphore, i.e., a semaphore with acquire and release operations
 * having arbitrary units value. Timeout on acquire is supported.
 * This implementation use a FIFO discipline satisfying acquires in order to avoid thread starvation
 * and use execution delegation in order to improve notification efficiency
 */
public class CounterSemaphore3{
    private Object monitor = new Object();

    /**
     * inner class to represent a node in the requests queue
     * Flag done note that the required units have been given by  a previous release operation
     */
    private  static class  Request {
        boolean done;
        int reqUnits;

        public Request(int n) {
            reqUnits = n;
        }
    }

    private List<Request> requests;
    private int units;

    /**
     * Auxiliary method to satisfy requests
     * while possible
     * @param n increase units by n
     */
    private void doRelease(int n) {
        int waked = 0;
        units += n;
        while(requests.size() == 0) {
            Request r = requests.get(0);
            if (units < r.reqUnits ) return;
            units -= r.reqUnits;
            r.done = true;
            requests.remove(0);
        };
        if (waked > 0) monitor.notifyAll();
    }

    // public interface
    public CounterSemaphore3(int initialCount) {
        requests = new LinkedList<>();
        units = initialCount;
    }

    public void release(int n)   {
        synchronized(monitor) {
            doRelease(n);
        }
    }

    public boolean acquire(int n, long timeout) throws InterruptedException {
        synchronized(monitor) {
            // fast path to success or fail on timeout
            if (requests.size() == 0 && units >= n) {
                units -= n;
                return true;
            }
            if (timeout == 0)
                return false;
            // prepare wait
            Request req = new Request(n);
            requests.add(req);
            do {
                try {
                    long refTime = System.currentTimeMillis();
                    monitor.wait();
                    // If request.done there is nothing to do by the waiter.
                    // Just return success
                    if (req.done) return true;
                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0)
                        // timeout ocurrs
                        return false;
                } catch (InterruptedException e) {
                    if (req.done) {
                        // Now, we have to possibilities, return with success delaying the interrution, or
                        // undo the operation and rethrow exception.
                        // With execution delegation the first is always possible and is used on the majority of cases
                        // but sometimes the second (undo) is possible and even desirable. The semaphore is such a case.
                        // to undo just use auxiliary method doRelease
                        doRelease(n);

                        // if we use the first option (return with success delaying the interruption
                        // the code was simply:
                        // Thread.currentThread().interrupt();
                        // return true;
                    } else {
                        // if done is false the request still is in the requests queue
                        // we must remove it before rethrow exception
                        requests.remove(req);
                    }
                    throw e;
                }

            } while (true);
        }
    }

    /**
     * Overloaded Acquire without timeout
     * @param n units to acquire
     * @throws InterruptedException
     */
    public void acquire( int n) throws InterruptedException {
        acquire(n, INFINITE);
    }
}
