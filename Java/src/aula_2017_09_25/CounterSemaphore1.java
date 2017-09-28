package aula_2017_09_25;

import static utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;

/**
 * Created by jmartins on 27/09/2017 (based on Carlos Martins implementation)
 *
 * This monitor implements a counter semaphore, i.e., a semaphore with acquire and release operations
 * having and arbitrary units value. Timeout on acquire is supported.
 *
 * This implementation simply use a counter with the current semaphore units
 * and acquisition is satisfied when semaphore units are sufficient ( semaphore units >= request units)
 * This semantic legitimate barging, with possible starvation of threads with big acquire requests.
 */
public class CounterSemaphore1 {
    private Object monitor = new Object();

    private int units;

    public CounterSemaphore1(int initial) {
        units = initial;
    }

    /**
     * Release is simple
     * Just increase semaphore units and notify all waiting threads
     * @param n the units to release
     */
    public void Release(int n) {
        synchronized (monitor) {
            units += n;
            monitor.notifyAll();
        }
    }

    /**
     *  Acquire operation.
     *  There is no need to handle interruption since the notification
     *  was a broadcast one and wait operation doesn't change monitor state!
     * @param n the units to acquire
     * @param timeout max waiting time
     * @return  true if operation succeeded, false if timeout
     * @throws InterruptedException
     */
    public boolean Acquire(int n, long timeout) throws InterruptedException {
        synchronized(monitor) {
            // first try a fast path
            if (units >= n) {
                //success!
                units -= n;
                return true;
            }
            if (timeout == 0) {
                // fail!
                return false;
            }
            // enter wait loop
            do {
                long refTime = System.currentTimeMillis();
                if (timeout == INFINITE)
                    monitor.wait();
                else
                    monitor.wait(timeout);
                if (units >= n) {
                    //success!
                    units -= n;
                    return true;
                }
                timeout = remainingTimeout(refTime,timeout);
                if (timeout == 0) return false;
            }
            while(true);
        }
    }

}
