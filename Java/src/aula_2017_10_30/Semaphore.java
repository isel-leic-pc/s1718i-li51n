package aula_2017_10_30;

import java.util.concurrent.atomic.AtomicInteger;

import static utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;

public class Semaphore {

    private Object monitor = new Object();

    private AtomicInteger permits; // number of semaphore units
    private volatile int waiters;

    public Semaphore(int initial) {
        permits = new AtomicInteger(initial);

    }

    private boolean tryAcquire() {
        int p;
        do {
            p = permits.get();
            if (p == 0) return false;

        }while(!permits.compareAndSet(p,p -1 ));
        return true;
    }

    public boolean acquire( long timeout) throws InterruptedException{
        // fast path lock-free
        if (tryAcquire()) return true;
        if (timeout == 0) return false;

        synchronized(monitor) {


            while(true) {
                long refTime = System.currentTimeMillis();
                waiters++;

                if (tryAcquire()) {
                    waiters--;
                    return true;
                }
                try {
                    if (timeout == INFINITE)
                        monitor.wait();
                    else
                        monitor.wait(timeout);
                    if (tryAcquire()) return true;

                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0)
                        return false;

                }
                catch(InterruptedException e) {
                    if (permits.get() > 0) monitor.notify();
                    throw e;
                }
                finally {
                    waiters--;
                }

            }
        }
    }
    public void acquire() throws InterruptedException {
        acquire(INFINITE);
    }

    public void release() {
        permits.incrementAndGet();

        if (waiters > 0) {
            synchronized(monitor) {
                if (waiters > 0)
                    monitor.notify();
            }
        }

    }

}
