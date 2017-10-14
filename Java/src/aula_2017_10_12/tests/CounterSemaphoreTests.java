package aula_2017_10_12.tests;

import aula_2017_10_12.CounterSemaphoreEN;
import org.junit.Assert;
import org.junit.Test;

public class CounterSemaphoreTests {


    private class TestThread  extends Thread {
        CounterSemaphoreEN sem;
        int reqUnits;
        boolean memInterrupt;
        boolean interrupted;

        TestThread(CounterSemaphoreEN s, int n, boolean mem) {
            sem = s;
            reqUnits = n;
            memInterrupt = mem;
            interrupted = false;
        }

        public void run() {
            try {
                sem.acquire(reqUnits);
            } catch (InterruptedException e) {
                if (memInterrupt)
                    interrupted = true;
            }
        }
    }

    @Test
    public void CheckRemoveRequestOnTimeout() throws InterruptedException{
        CounterSemaphoreEN sem = new CounterSemaphoreEN(0);

        boolean res = sem.acquire(1, 1000);
        Assert.assertTrue(!res && sem.getUnits() == 0 && sem.getWaitersCount() == 0);
    }

    @Test
    public void CheckFIFO_OnMultipleAcquisition() throws InterruptedException{
        CounterSemaphoreEN sem = new CounterSemaphoreEN(0);

        Thread t1 = new TestThread(sem, 4, false);
        t1.start();

        Thread t2 = new TestThread(sem, 4, false);
        t2.start();
        // synchronize with the waiting of the threads
        while (sem.getWaitersCount() != 2) {
            try { Thread.sleep(1000); } catch(InterruptedException e) {}
        }

        TestThread t3 = new TestThread(sem, 4, true);
        t3.start();
        // synchronize with the waiting of the last thread
        while (sem.getWaitersCount() != 3) {
            try { Thread.sleep(1000); } catch(InterruptedException e) {}
        }
        // awake first two threads
        sem.release(10);
        t1.join();
        t2.join();

        // check current semaphore state
        boolean res = sem.getUnits() == 2 && sem.getWaitersCount() == 1;

        t3.interrupt();
        t3.join();
        Assert.assertTrue(res && t3.interrupted);
    }
}

