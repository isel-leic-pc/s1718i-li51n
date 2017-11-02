package aula_2017_10_30;

import static utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;

public class Semaphore {

    private Object monitor = new Object();

    private int permits; // number of semaphore units


    public Semaphore(int initial) {
        permits = initial;

    }



    public boolean acquire( long timeout) throws InterruptedException{
        synchronized(monitor) {
            if (permits > 0) {
                permits -= 1;
                return true;
            }
            if (timeout == 0) {
                return false;
            }


            while(true) {
                long refTime = System.currentTimeMillis();
                try {
                    if (timeout == INFINITE)
                        monitor.wait();
                    else
                        monitor.wait(timeout);
                    if (permits > 0) {
                        permits-= 1;
                        return true;
                    }
                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0)
                        return false;

                }
                catch(InterruptedException e) {
                    if (permits > 0) monitor.notify();
                    throw e;
                }

            }
        }
    }
    public void acquire() throws InterruptedException {
        acquire(INFINITE);
    }

    public void release() {
        synchronized(monitor) {
            permits += 1;
            monitor.notify();
        }
    }

}
