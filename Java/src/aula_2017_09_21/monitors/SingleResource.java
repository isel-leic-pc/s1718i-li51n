package aula_2017_09_21.monitors;

import utils.SynchUtils;

import static utils.SynchUtils.INFINITE;

/**
 * A monitor to provide exclusive access to a resource
 * Simplified version, without interruption handling and timeout on Acquire operation
 */
public class SingleResource  {
    private boolean busy = false;
    private Object monitor = new Object();


    public boolean Acquire(long timeout) throws InterruptedException {
        synchronized(monitor) {
            if (!busy) {
                busy = true;
                return true;
            }
            if (timeout == 0) {
                return false;
            }
            do {
                try {
                    long refTime = System.currentTimeMillis();
                    if (timeout == SynchUtils.INFINITE)
                        monitor.wait();
                    else
                        monitor.wait(timeout);
                    if (!busy) {
                        busy = true;
                        return true;
                    }
                    timeout = SynchUtils.remainingTimeout(refTime, timeout);
                    if (timeout == 0)
                        return false;
                }
                catch(InterruptedException e) {
                    /* opção A : considerar possivel situação de
                       sucesso

                    if (busy == false) {
                        Thread.currentThread().interrupt();
                        busy = true;
                        return true;
                    }
                    throw e;
                    */

                    /* opção B - relançar sempre a excepção *7

                     */
                    if (busy == false)
                        monitor.notify();
                    throw e;
                }
            } while(true);

        }
    }

    public boolean Acquire() throws InterruptedException {
        return Acquire(INFINITE);
    }
}

