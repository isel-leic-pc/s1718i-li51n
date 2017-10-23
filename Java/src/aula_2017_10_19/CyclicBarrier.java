package aula_2017_10_19;

/**
 * Created by jmartins on 02/10/2017.
 *
 * This synchronizer has no complication because it can be used just for one synch round
 */
public class CyclicBarrier {
    private Object monitor = new Object(); // the monitor object

    private int parts;
    private int remaining;
    private boolean broken;

    public static class BrokenBarrierException extends Exception {}

    public CyclicBarrier(int parts) {
        if (parts <= 0) throw new IllegalStateException();

        this.parts = remaining = parts;
    }

    /**
     * Assumed to run inside monitor
     * @return
     */
    private boolean checkLastPart() {
        //if (remaining == 0) throw new IllegalStateException();
        if (--remaining == 0) {
            monitor.notifyAll();
            remaining = parts;
            return true;
        }
        return false;
    }

    public void await()
            throws IllegalStateException, InterruptedException,
            BrokenBarrierException
    {
        synchronized(monitor) {
            if (broken) throw new BrokenBarrierException();
            if (checkLastPart()) return;

            while(remaining > 0) {
                try {
                    monitor.wait();
                    if (broken) throw new BrokenBarrierException();
                }
                catch(InterruptedException e) {
                    broken=true;
                    if (remaining < parts -1)
                        monitor.notifyAll();
                }
            }
        }
    }

}