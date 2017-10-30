/**
 * Implementation of manual reset event synchronizer
 * In this case, execution delegation technique is mandatory
 *
 * Jmartins, october 2017
 */
package aula_2017_10_19;

import java.util.LinkedList;
import java.util.List;

import static utils.SynchUtils.remainingTimeout;

public class ManualResetEventBatch {
    private Object monitor = new Object();  // the monitor
    private boolean signaled;               // the event state

    private int signalVersion;

    public ManualResetEventBatch(boolean initialState) {
        signaled = initialState;
    }


    public boolean await(long timeout) throws InterruptedException {
        synchronized(monitor) {
            // fast path
            if (signaled) return true;
            if (timeout == 0) return false;

            // prepare wait
            int currentVersion = signalVersion;
            do {

                long refTime = System.currentTimeMillis();
                monitor.wait(timeout);
                if (currentVersion != signalVersion) return true;
                timeout = remainingTimeout(refTime, timeout);
                if (timeout == 0) {
                    // abort operation on timeout
                    return false;
                }
            }
            while(true);
        }
    }

    public void set()  {
        synchronized(monitor) {
            if (!signaled) {
                signalVersion++;
                signaled=true;
                monitor.notifyAll();
            }
        }
    }

    public void reset()  {
        synchronized(monitor) {
            signaled = false;
        }
    }
}
