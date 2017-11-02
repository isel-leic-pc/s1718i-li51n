package aula_2017_10_30.tests;

import aula_2017_10_30.Semaphore;
import org.junit.Assert;
import org.junit.Test;

import java.util.concurrent.CyclicBarrier;

public class SemaphoreTests {
    private final int turns = 2000000;
    private final int nthreads = 10;
    private final int acquire_timeout = 2000;
    private final int ntries = 4;

    int count = 0;
    volatile boolean error;

    @Test
    public void testAsCriticalSection() throws InterruptedException {
        Semaphore s = new Semaphore(1);
        CyclicBarrier barrier = new CyclicBarrier(nthreads + 1);

        Runnable tfunc = () -> {
            try {

                barrier.await();
                for (int i = 0; i < turns; ++i) {

                    if (!s.acquire(acquire_timeout)) {
                        error = true;

                        return;
                    }
                    count++;
                    s.release();
                }
            } catch (Exception e) {
                error = true;
            }

        };


        for (int i = 0; i < ntries; ++i) {
            Thread[] threads = new Thread[nthreads];

            try {
                count = 0;
                for (int t = 0; t < nthreads; t++) {
                    threads[t] = new Thread(tfunc);
                    threads[t].start();
                }
                barrier.await();
            } catch (Exception e) {
                error = true;
            }

            for (int t = 0; t < nthreads && !error; t++)
                threads[t].join();

            Assert.assertFalse(error);
            Assert.assertEquals(count, turns * nthreads);
        }

    }
}
