package exercicios1;

/**
 * Created by jmartins on 02/10/2017.
 *
 * This synchronizer has no complication because it can be used just for one synch round
 */
public class PhasedGate {
    private Object monitor = new Object(); // the monitor object

    private int remaining;

    public PhasedGate(int parts) {
         remaining = parts;
    }

    /**
     * Assumed to run inside monitor
     * @return
     */
    private boolean checkLastPart() {
        if (remaining == 0) throw new IllegalStateException();
        if (--remaining == 0) {
            monitor.notifyAll();
            return true;
        }
        return false;
    }

    public void await() throws IllegalStateException, InterruptedException {
        synchronized(monitor) {
            if (checkLastPart()) return;

            while(remaining > 0) {
                monitor.wait();
            }
        }
    }

    public void removeParticipant() {
        synchronized(monitor) {
            checkLastPart();
        }
    }
}