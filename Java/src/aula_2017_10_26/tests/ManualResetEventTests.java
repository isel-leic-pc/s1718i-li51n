package aula_2017_10_26.tests;

import aula_2017_10_26.ManualResetEvent;
import org.junit.Assert;
import org.junit.Test;

import java.util.concurrent.*;

public class ManualResetEventTests {


    private static class Result {

        volatile boolean intrExceptionOnBarrier;
        volatile boolean brokenExceptionOnBarrier;
        boolean setterExceptionOnBarrier;

        int[] waitScores = new int[WAITERS];
        int setterScores;

        public boolean ok() {
            return
                    !this.intrExceptionOnBarrier &&
                    !this.brokenExceptionOnBarrier &&
                    !this.setterExceptionOnBarrier;
        }
    }
    private static final int PARTS = 11;
    private static final int WAITERS = PARTS -1;
    private static final int NROUNDS=20000;
    private static final int WAIT_ROUND_TIMEOUT= 10000;
    private static final int TRIES= 100;

    private volatile CountDownLatch cdl;

    @Test
    public void multipleWaitersTest () {

        for(int tr = 0; tr < TRIES; ++tr) {
            CyclicBarrier cb = new CyclicBarrier(PARTS);
            Thread[] threads = new Thread[WAITERS];
            Result res = new Result();
            ManualResetEvent mre = new ManualResetEvent(false);

            for (int i= 0; i < WAITERS; ++i) {
                final int li = i;
                Thread t = new Thread(() -> {
                    for(int r = 0; r < NROUNDS && res.ok() ; ++r) {
                        try {
                            cb.await();
                            mre.await();
                            cdl.countDown();
                            res.waitScores[li]++;
                        }
                        catch (InterruptedException e) {
                            res.intrExceptionOnBarrier  = true;
                        }
                        catch (BrokenBarrierException e) {
                            res.brokenExceptionOnBarrier  = true;
                        }
                    }
                });
                t.start();
                threads[i] = t;
            }

            for(int r = 0; r < NROUNDS && res.ok(); ++r) {
                try {
                    cb.await(WAIT_ROUND_TIMEOUT, TimeUnit.MILLISECONDS);
                    cdl = new CountDownLatch(WAITERS);
                    mre.set();
                    if (!cdl.await(WAIT_ROUND_TIMEOUT, TimeUnit.MILLISECONDS))
                        res.setterExceptionOnBarrier = true;

                    res.setterScores++;
                    mre.reset();
                } catch (Exception e) {
                    res.setterExceptionOnBarrier = true;
                    break;
                }
            }

            boolean okWaiterCounters=res.ok();
            for(int w = 0; w < WAITERS && okWaiterCounters ; ++w) {
                try {
                    threads[w].join();
                    okWaiterCounters = okWaiterCounters && res.waitScores[w] == NROUNDS;
                }
                catch(InterruptedException e) {

                }
            }


            Assert.assertFalse(res.brokenExceptionOnBarrier);
            Assert.assertFalse(res.intrExceptionOnBarrier);
            Assert.assertFalse(res.setterExceptionOnBarrier);
            Assert.assertTrue(okWaiterCounters && res.setterScores == NROUNDS);
            Assert.assertTrue(mre.getWaiters() == 0);
        }

    }
}
