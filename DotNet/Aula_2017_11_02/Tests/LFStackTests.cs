using NUnit.Framework;
using System;

using System.Threading;

namespace Aula_2017_11_02 {
    public class LFStackTests {
        private const int nthreads = 10;
        private const int nvals = 10000;

        private class ThreadCounter {
            public readonly int id;
            public int count;

            internal ThreadCounter(int id) { this.id = id; }

        }

        private int addCounter(ThreadCounter[] counters, int nc, int id) {
            for (int j = 0; j < nc; j++) {
                if (counters[j].id == id) {
                    counters[j].count++;
                    return nc;
                }
            }
            counters[nc] = new ThreadCounter(id);
            return nc + 1;
        }

        private void showAllThreads(int[] tids) {
            ThreadCounter[] counters = new ThreadCounter[nthreads];
            int nc = 0;

            for (int i = 0; i < nvals; ++i) {
                int id = tids[i];
                nc = addCounter(counters, nc, id);
            }

            for (int i = 0; i < nthreads; ++i) {
                if (counters[i] != null)
                    Console.WriteLine("Thread {0} -- {1}", counters[i].id, counters[i].count);
            }
        }

        [Test]
        public void PopAllTest() {
            Barrier barrier = new Barrier(nthreads + 1);

            LFStack<int> s = new LFStack<int>();
            bool[] vals = new bool[nvals];
            int[] tids = new int[nvals];
            Thread[] threads = new Thread[nthreads];

            for (int i = 0; i < nvals; ++i) {
                s.Push(i);
            }

            for (int i = 0; i < nthreads; ++i) {
                Thread t = new Thread(() => {
                    barrier.SignalAndWait();
                    int oldv = nvals, v;
                 
                    while (s.Pop(out v)) {
                        if (v >= oldv) break;
                        vals[v] = true;
                        tids[v] = Thread.CurrentThread.ManagedThreadId;
                        oldv = v;
                    }

                });
                threads[i] = t;
                t.Start();
            }
             
           
           
            barrier.SignalAndWait(); // start all!


            for (int i = 0; i < nthreads; ++i) {
                threads[i].Join();
            }

            showAllThreads(tids);
            for (int i = 0; i < nvals; ++i) {
                Assert.IsTrue(vals[i]);
            }
        }
    }
}
