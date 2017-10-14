/**
 * CounterSemaphore implementaion using execution delegation and explicit notification
 * supported by multiple conditions
 */
package aula_2017_10_12;

import java.util.LinkedList;
import java.util.List;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.ReentrantLock;

import static utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;

public class CounterSemaphoreEN {
    private ReentrantLock monitor = new ReentrantLock();
    private int units;

    private class Request {
        int reqUnits;
        boolean done;
        Condition cond;

        public Request(int n) {
            reqUnits = n;
            cond = monitor.newCondition();
        }
    }

    private List<Request> requests;

    public CounterSemaphoreEN(int initial) {
        units = initial;
        requests = new LinkedList<Request>();
    }

    public boolean acquire(int n, long timeout) throws InterruptedException {
        monitor.lock();

        try {
            if (requests.size() == 0 && units >= n) {
                units -= n;
                return true;
            }
            if (timeout == 0) {
                return false;
            }

            Request req = new Request(n);
            requests.add(req);
            do {
                try {
                    long refTime = System.currentTimeMillis();
                    req.cond.await(timeout, TimeUnit.MILLISECONDS);
                    if (req.done) return true;
                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0) {
                        requests.remove(req);
                        return false;
                    }
                } catch (InterruptedException e) {
                    if (req.done) {
                        Thread.currentThread().interrupt();
                        return true;
                    }
                    requests.remove(req);
                    throw e;
                }
            }
            while (true);
        } finally {
            monitor.unlock();
        }
    }

    public void release(int n) {
        monitor.lock();
        try {
            units += n;
            while (requests.size() > 0) {
                Request r = requests.get(0);
                if (r.reqUnits > units)
                    return;
                requests.remove(0);
                units -= r.reqUnits;
                r.done = true;
                r.cond.signal();
            }
        } finally {
            monitor.unlock();
        }
    }

    public void acquire(int units) throws InterruptedException {
        acquire(units, INFINITE);
    }

    public int getWaitersCount() {
        monitor.lock();
        try {
            return requests.size();
        } finally {
            monitor.unlock();
        }
    }

    public int getUnits() {
        monitor.lock();
        try {
            return units;
        } finally {
            monitor.unlock();
        }
    }
}