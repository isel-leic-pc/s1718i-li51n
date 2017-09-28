package aula_2017_09_21.monitors;

import java.util.LinkedList;
import java.util.List;

/**
 * A monitor to provide synchronized access to a bounded queue
 * Simplified version, without interruption handling and timeout on Get operation
 */
public class BoundedQueue<T> {
    // use a private monitor to avoid external exposing
    private Object monitor = new Object();
    List<T> items;
    private int maxItems;

    public BoundedQueue(int maxItems) {
        this.maxItems = maxItems;
        items = new LinkedList<>();
    }

    public void Put(T item) throws InterruptedException{
        synchronized(monitor) {
            while(items.size() == maxItems)
                monitor.wait();
            items.add(item);
            // in order to wakeup a possible waiter on Get operation
            monitor.notifyAll();
        }
    }

    public T Get() throws InterruptedException {
        synchronized (monitor) {
            while (items.size() == 0)
                monitor.wait();
            // remove and retrieve item at queue head
            T item = items.remove(0);
            // in order to wakeup a possible waiter on Put operation
            monitor.notifyAll();
            return item;
        }
    }

}
