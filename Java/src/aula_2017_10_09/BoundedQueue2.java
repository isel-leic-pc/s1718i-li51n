package aula_2017_10_09;

import java.util.LinkedList;
import java.util.List;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.ReentrantLock;

import static utils.SynchUtils.remainingTimeout;

/**
 * Bounded queue using explicit locks with multiple conditions
 * Created by jmartins on 09/10/2017.
 */
public class BoundedQueue2<T> {
    // use a private monitor to avoid external exposing
    private ReentrantLock monitor = new ReentrantLock();
    Condition hasSpace, hasItems;

    List<T> items;
    private int maxItems;

    /**
     * runs inside the monitor
     * add a new item  at queue tail if it is not full
     * @param item
     * @return
     */
    private boolean doPut(T item) {
        if (items.size() < maxItems) {
            items.add(item);
            // in order to wakeup a possible waiter on get operation
            hasItems.signal();
            return true;
        }
        return false;
    }

    /**
     * runs inside the monitor
     * remove and retrieve item at queue head if it is not empty
     * @return
     */
    private T doGet() {
        //
        if (items.size() > 0) {
            T item = items.remove(0);
            // in order to wakeup a possible waiter on put operation
            hasSpace.signal();
            return item;
        }
        return null;
    }

    public BoundedQueue2(int maxItems) {
        this.maxItems = maxItems;
        items = new LinkedList<>();
        hasSpace =  monitor.newCondition();
        hasItems = monitor.newCondition();
    }

    public void put(T item) throws InterruptedException{
        monitor.lock();
        try {
            if (doPut(item)) return;
            do {
                try {
                    hasSpace.await();
                    if (doPut(item)) return;
                }
                catch(InterruptedException e) {
                    if (items.size() < maxItems) {
                        // necessary to avoid lost notification
                        hasSpace.signal();
                    }
                    throw e;
                }
            }
            while(true);
        }
        finally {
            monitor.unlock();
        }
    }

    public T get(long timeout) throws InterruptedException {
        monitor.lock();
        try {
            T item;
            if ((item = doGet()) != null) return item;
            do {
                try {
                    long refTime= System.currentTimeMillis();
                    hasItems.await(timeout, TimeUnit.MILLISECONDS);
                    if ((item = doGet()) != null) return item;
                    timeout = remainingTimeout(refTime,timeout);
                    if (timeout == 0) // end operation on timeout
                        return null;
                }
                catch(InterruptedException e) {
                    // necessary to avoid lost notification
                    hasItems.signal();
                }
            }
            while(true);
        }
        finally {
            monitor.unlock();
        }
    }

}
