package aula_2017_09_27;

/**
 * Created by jmartins on 28/09/2017.
 */

import java.util.LinkedList;
import java.util.List;

/**
 * Counter semaphore made with explicit monitor (ReentrantLock)
 * and execution delegation
 */
public class CounterSemaphore3{
    private Object monitor = new Object();

    private  class  Request {
        boolean done;
        int reqUnits;

        public Request(int n) {
            reqUnits = n;
        }
    }

    private List<Request> requests;
    private int units;

    public CounterSemaphore3(int initialCount) {
        requests = new LinkedList<>();
        units = initialCount;
    }

    public void Acquire(int n) throws InterruptedException {
        synchronized(monitor) {
            if (requests.size() == 0 && units >= n) {
                units -= n;
                return;
            }
            Request req = new Request(n);
            requests.add(req);
            do {
                try {
                    monitor.wait();
                    if (req.done) return;
                } catch (InterruptedException e) {
                    if (req.done) {
                        units += req.reqUnits;
                        Thread.currentThread().interrupt();
                        return;
                    }
                    throw e;
                }

            } while (true);


    }

    public void Release(int n)   {
        synchronized(monitor) {
            units += n;

            while(requests.size() == 0) {
                Request r = requests.get(0);
                if (units < r.reqUnits ) return;
                units -= r.reqUnits;
                r.done = true;
                r.cond.signal();

                requests.remove(0);
            };

        }
    }
}
