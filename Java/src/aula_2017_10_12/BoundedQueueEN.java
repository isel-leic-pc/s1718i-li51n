/**
 * BoundedQueue with separated conditions for
 * space available wait/notification  and item available wait/notification
 */
package aula_2017_10_12;

import java.util.LinkedList;
import java.util.List;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.Lock;
import java.util.concurrent.locks.ReentrantLock;

import static utils.SynchUtils.INFINITE;
import static utils.SynchUtils.remainingTimeout;

public class BoundedQueueEN<T> {
    private Lock monitor = new ReentrantLock();
    private Condition hasSpace, hasItems;

    private List<T> items;
    private int maxItems;

    public BoundedQueueEN(int maxItems) {
        this.maxItems = maxItems;
        items = new LinkedList<>();
        hasSpace = monitor.newCondition();
        hasItems = monitor.newCondition();
    }

    public void put(T item) throws InterruptedException{
        monitor.lock();
        try {
            do {
                try {
                    while (items.size() == maxItems)
                        hasSpace.await();
                    items.add(item);
                    hasItems.signal();
                }
                catch(InterruptedException e) {
                    if (items.size() < maxItems)
                        // to avoid notification lost in case interruption occurs simultaneously with notification
                        hasSpace.signal();
                    throw e;
                }
            }
            while(true);
        }
        finally {
            monitor.unlock();
        }
    }

    public T get(long timeout) throws InterruptedException{
        monitor.lock();
        try {
            if (items.size() > 0) {
                T item = items.remove(0);
                hasSpace.signal();
                return item;
            }
            if (timeout == 0)
                return null;
            do {
                try {
                    long refTime = System.currentTimeMillis();
                    if (timeout == INFINITE)
                        hasItems.await(timeout, TimeUnit.MILLISECONDS);
                    else {
                        hasItems.await();
                    }
                    if (items.size() > 0) {
                        T item = items.remove(0);
                        hasSpace.signal();
                        return item;
                    }
                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0) {
                        return null;
                    }
                }
                catch(InterruptedException e) {
                    if (items.size() > 0)
                        // to avoid notification lost in case interruption occurs simultaneously with notification
                        hasSpace.signal();
                    throw e;
                }
            }
            while (true);
        }
        finally {
            monitor.unlock();
        }
    }

}
