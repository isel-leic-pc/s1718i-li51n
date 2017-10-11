package exercicios1;
import sun.plugin.dom.exception.InvalidStateException;

import static utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;


/**
 * Created by jmartins on 28/09/2017.
 */
public class FutureHolder<T> {
    private Object monitor = new Object();
    private T value;

    public void setValue(T val) {
        synchronized(monitor) {
            if (value != null || val == null) throw new InvalidStateException("Value already exists or is null!");
            value = val;
            monitor.notifyAll();
        }
    }

    public T getValue(long timeout) throws InterruptedException {
        synchronized(monitor) {
            if (value != null) return value;
            if (timeout == 0) return null;
            do {
                long refTime = System.currentTimeMillis();
                if (timeout == INFINITE)
                    monitor.wait();
                else
                    monitor.wait(timeout);
                if (value != null) return value;
                timeout = remainingTimeout(refTime, timeout);
                if (timeout ==0) return null;
            }
            while(true);
        }
    }

    public boolean isValueAvailable() {
        synchronized(monitor) {
            return value != null;
        }
    }

}

