package aula_2017_10_19;

/**
 * Created by jmartins on 19/10/2017.
 *
 *  Partial implementation of CyclicBarrier synchronizer
 *  (to be concluded)
 */
public class CyclicBarrier {
    private Object monitor = new Object(); // the monitor object

    private int parts;
    private int remaining;
    private boolean broken;

    private int roundVersion;

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
            roundVersion++;
            remaining = parts;
            monitor.notifyAll();

            return true;
        }
        return false;
    }

    public void await()
            throws InterruptedException,
            BrokenBarrierException
    {
        synchronized(monitor) {
            if (broken) throw new BrokenBarrierException();
            if (checkLastPart()) return;
            int currentVersion = roundVersion;
            do  {
                try {
                    monitor.wait();
                    if (currentVersion != roundVersion) return;
                    if (broken) throw new BrokenBarrierException();
                }
                catch(InterruptedException e) {
                    broken=true;
                    if (remaining < parts -1)
                        monitor.notifyAll();
                }
            }
            while(true);
        }
    }

}