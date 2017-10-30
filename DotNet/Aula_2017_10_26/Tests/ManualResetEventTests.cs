using NUnit.Framework;
using System;
 
using System.Threading;
using Aula_2017_10_26.LockFree;

namespace Aula_2017_10_26.Tests {
    public class ManualResetEventTests {
        private const int PARTS = 11;
        private const int WAITERS = PARTS - 1;
        private const int NROUNDS = 50000;
        private const int NITERS = 10;
        private const int WAIT_ROUND_TIMEOUT = 10000;

        private class Result {
            internal volatile bool timeoutExceptionOnBarrier;
            internal volatile bool intrExceptionOnBarrier;
            internal volatile bool brokenExceptionOnBarrier;
            internal bool setterExceptionOnBarrier;

            internal int[] waitScores = new int[WAITERS];
            internal int setterScores;

            public bool ok() {
                return !this.timeoutExceptionOnBarrier &&
                        !this.intrExceptionOnBarrier &&
                        !this.brokenExceptionOnBarrier &&
                        !this.setterExceptionOnBarrier;
            }
        }


        private volatile CountdownEvent cdl;

        [Test]
        public void multipleWaitersTest() {
           

            for (int n = 0; n < NITERS; ++n) {
                Barrier cb = new Barrier(PARTS);
                Thread[] threads = new Thread[WAITERS];
                Result res = new Result();
                var mre = new LockFree.ManualResetEvent(false);

                for (int i = 0; i < WAITERS; ++i) {
                    int li = i;
                    Thread t = new Thread(() => {
                        for (int r = 0; r < NROUNDS && res.ok(); ++r) {
                            try {
                                cb.SignalAndWait(WAIT_ROUND_TIMEOUT);
                                mre.await(Timeout.Infinite);
                                cdl.Signal();
                                res.waitScores[li]++;
                            }
                            catch (TimeoutException  ) {
                                res.timeoutExceptionOnBarrier = true;
                            }
                            catch (ThreadInterruptedException ) {
                                res.intrExceptionOnBarrier = true;
                            }
                            catch (BarrierPostPhaseException  ) {
                                res.brokenExceptionOnBarrier = true;
                            }
                        }
                    });
                    t.Start();
                    threads[i] = t;
                }

                for (int r = 0; r < NROUNDS && res.ok(); ++r) {
                    try {
                        cb.SignalAndWait(WAIT_ROUND_TIMEOUT);
                        cdl = new CountdownEvent(WAITERS);
                        mre.set();
                        if (!cdl.Wait(WAIT_ROUND_TIMEOUT))
                            res.setterExceptionOnBarrier = true;

                        res.setterScores++;
                        mre.reset();
                    }
                    catch (Exception  ) {
                        res.setterExceptionOnBarrier = true;
                    }
                }

                bool okWaiterCounters = res.ok();
                for (int w = 0; w < WAITERS && okWaiterCounters; ++w) {
                    try {
                        threads[w].Join();
                        okWaiterCounters = okWaiterCounters && res.waitScores[w] == NROUNDS;
                    }
                    catch (ThreadInterruptedException  ) {

                    }
                }


                Assert.IsFalse(res.brokenExceptionOnBarrier);
                Assert.IsFalse(res.intrExceptionOnBarrier);
                Assert.IsFalse(res.timeoutExceptionOnBarrier);
                Assert.IsFalse(res.setterExceptionOnBarrier);
                Assert.IsTrue(okWaiterCounters && res.setterScores == NROUNDS);
                Assert.IsTrue(mre.getWaiters() == 0);
            }

        }
    }
}
