package monitors;

/**
 * A monitor to provide exclusive access to a resource
 * Simplified version, without interruption handling and timeout on Acquire operation
 */
public class SingleResource  {
    private boolean busy = true;
    private Object monitor = new Object();


    public void Acquire() throws InterruptedException {
        synchronized(monitor) {
            while (busy)
                 monitor.wait();
            busy = true;
        }
    }



    public void Release()  {
        synchronized(monitor) {
            busy = false;
            monitor.notify();
        }
    }

}

