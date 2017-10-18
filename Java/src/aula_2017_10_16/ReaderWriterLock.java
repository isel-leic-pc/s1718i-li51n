/**
 * ReaderWriterLock synchronizer
 * partial implementation
 *
 * Jmartins, october 2017
 */

package aula_2017_10_16;

import java.util.LinkedList;
import java.util.List;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.ReentrantLock;

import static utils.SynchUtils.remainingTimeout;

public class ReaderWriterLock  {
    private ReentrantLock monitor = new ReentrantLock(); // the monitor

    private int readers = 0;            // number of active readers
    private boolean writing = false;    // a writer is in place

    private List<ReadWaiter> waitingReaders;    // readers waiting queue
    private List<WriteWaiter> waitingWriters;   // writers waiting queue

    private Condition canRead;      // to allow broadcast wakeup of awaiting readers

    public ReaderWriterLock() {
        waitingReaders = new LinkedList<>();
        waitingWriters = new LinkedList<>();
        canRead = monitor.newCondition();
    }

    // auxliary classes for waiting queues
    private static class ReadWaiter {
        public boolean done;
    }

    private class WriteWaiter {
        Condition canWrite; // to permitic especific notification of the associated writer
        boolean done;

        public WriteWaiter() {
            canWrite = monitor.newCondition();
        }
    }

    // auxiliary methods

    private void tryAwakeWriter() {
        if (waitingWriters.size() > 0) {
            WriteWaiter ww = waitingWriters.remove(0); // remove the front element
            ww.done = true;
            writing = true;     // for all purposes a writer is active
            ww.canWrite.signal();
        }
    }

    private boolean tryAwakeReaders() {
        if (waitingReaders.size() > 0) {
            readers = waitingReaders.size();
            // the readers are already considered active as a result of execution delegation
            for(ReadWaiter rw: waitingReaders) rw.done=true;
            waitingReaders.clear();

            canRead.signalAll();
            return true;
        }
        return false;
    }


    /**
     * can enter if !writing
     */
    public boolean startRead(long timeout) throws InterruptedException {
        monitor.lock();
        try  {
            if (!writing && waitingWriters.size() == 0) {
                // fast path to success, note th epriority to waitingWriters to avoid read starvation
                readers++;
                return true;
            }
            if (timeout == 0) // fast path to fail
                return false;

            // prepare to wait
            ReadWaiter r = new ReadWaiter();
            waitingReaders.add(r);
            while(true) {
                try {
                    long refTime = System.currentTimeMillis();
                    canRead.await(timeout, TimeUnit.MILLISECONDS);
                    if (r.done) return true;
                    timeout = remainingTimeout(refTime, timeout);
                    if (timeout == 0) {
                        // abort operation on timeout
                        waitingReaders.remove(r);
                        return false;
                    }
                }
                catch(InterruptedException e) {
                    if (r.done) {
                        // in this case  delay interruption and return success
                        Thread.currentThread().interrupt();
                        return true;
                    }
                    waitingReaders.remove(r);
                    throw e;
                }

            }
        }
        finally {
            monitor.unlock();
        }
    }

    public void startWrite() throws InterruptedException {
        monitor.lock();
        try {
            if (!writing && readers == 0 && waitingWriters.size() == 0) {
                // note the FIFO discipline on write lock aquisition
                writing = true;
                return;
            }
            // prepare to wait
            WriteWaiter r = new WriteWaiter();
            waitingWriters.add(r);
            while(true) {
                try {
                    r.canWrite.await();
                    if(r.done) {
                        return;
                    }
                }
                catch(InterruptedException e) {
                    if(r.done) {
                        Thread.currentThread().interrupt();
                        return;
                    }
                    waitingWriters.remove(r);
                    throw e;
                }
            }
        }
        finally {
            monitor.unlock();
        }
    }

    public void endRead() {
        monitor.lock();

        try {
            readers--;
            if (readers == 0 )
                tryAwakeWriter();
        }
        finally {
            monitor.unlock();
        }


    }

    public void endWrite() {
        monitor.lock();
        try {
            writing = false;
            if (!tryAwakeReaders()) // readers first, to avoid starvation
                tryAwakeWriter();
        }
        finally {
            monitor.unlock();
        }
    }
}

