using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aula_2017_11_23 {
    class SortUtils {

        
        private const int ArraySize = 5000000;
        private const int Threshold = 4000;
        private const int MaxDepth = 4;

        private static int totaltasks;

        private class SynchTaskScheduler : TaskScheduler {
            protected override IEnumerable<Task> GetScheduledTasks() {
                throw new NotImplementedException();
            }

            protected override void QueueTask(Task task) {
                TryExecuteTask(task);
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
                throw new NotImplementedException();
            }
        }
        private static T[] CreateVals<T>() {

            T[] vals = new T[ArraySize];
            return vals;
        }


        private static int[] CreateVals() {

            int[] vals = new int[ArraySize];
            Random r = new Random();
            for (int i = 0; i < ArraySize; ++i) vals[i] = r.Next(1, ArraySize);
            return vals;
        }

        private class ThreadReport {
            private static List<ThreadReport> tlist = new List<ThreadReport>(1000);

            public static ThreadLocal<ThreadReport> current =
                new ThreadLocal<ThreadReport>(() => new ThreadReport());

            private static object _lock = new object();

            public static void RegistReport() {
                ThreadReport tr = current.Value;
                tr.nUses++;
                tr.lastUse = Environment.TickCount;
            }

            public static void AddReport(ThreadReport tr) {
                lock (_lock) {
                    tlist.Add(tr);
                }
            }

            private readonly Thread theThread;
            private readonly int id;
            private volatile int firstUse;
            private volatile int lastUse;
            private volatile int nUses;

            private ThreadReport() {
                theThread = Thread.CurrentThread;
                id = theThread.ManagedThreadId;
                firstUse = Environment.TickCount;
                AddReport(this);
            }

            private void Reset() {
                firstUse = Environment.TickCount;
                nUses = 0;

            }
            public override string ToString() {
                return String.Format("Thread {0} used {1} times, from {2} to {3}",
                    id, nUses, firstUse, lastUse);
            }
            public static void ShowStats() {
                int count = 0;
                foreach (ThreadReport tr in tlist) {

                    if (tr.nUses > 0) {
                        count++;
                        Console.WriteLine(tr);
                    }
                }
                Console.WriteLine("Used {0} threads!", count);
            }

            public static void ResetAll() {

                foreach (ThreadReport tr in tlist) {
                    tr.Reset();
                }
            }
        }

        private static void Partition<T>(T[] vals, T pivot, ref int min, ref int max)
            where T : IComparable<T> {
            do {
                while (vals[max].CompareTo(pivot) > 0) max--;
                while (vals[min].CompareTo(pivot) < 0) min++;
                if (max >= min) {
                    T tmp = vals[max];
                    vals[max] = vals[min];
                    vals[min] = tmp;
                    min++;
                    max--;
                }
            } while (max > min);
        }

        public static void QuickSort<T>(T[] vals, int min, int max)
           where T : IComparable<T> {
            int first = min, last = max;
            if (min >= max) return;

            T pivot = vals[(min + max) / 2];
            Partition(vals, pivot, ref min, ref max);

            QuickSort(vals, first, max);
            QuickSort(vals, min, last);

        }

        private static SynchTaskScheduler scheduler = new SynchTaskScheduler();

        public static void PQuickSort<T>(T[] vals, int min, int max, int depth) 
            where T : IComparable<T> {
            int first = min, last = max;

            if (min >= max) return;

            if ( (max - min) < Threshold) {
                QuickSort(vals, min, max);
                return;
            }


            T pivot = vals[(min + max) / 2];
            Partition(vals, pivot, ref min, ref max);

          
               
            Task t1 = Task.Factory.StartNew(
                () => {
                    //int tid = Thread.CurrentThread.ManagedThreadId;
                    //Console.WriteLine("-->Sort({0},{1}, #{2:00})", first, max, tid);
                    Interlocked.Increment(ref totaltasks);
                    ThreadReport.RegistReport();
                    PQuickSort(vals, first, max, depth + 1);
                    //Console.WriteLine("<--Sort({0},{1}, #{2:00})", first, max, tid);

                }, CancellationToken.None, TaskCreationOptions.None, scheduler);
                 
            Task t2 = Task.Factory.StartNew(
            () => {
                //int tid = Thread.CurrentThread.ManagedThreadId;
                //Console.WriteLine("-->Sort({0},{1}, #{2:00})", min, last, tid);
                Interlocked.Increment(ref totaltasks);
                ThreadReport.RegistReport();
                PQuickSort(vals, min, last, depth + 1);
               
                //Console.WriteLine("<--Sort({0},{1}, #{2:00})", min, last, tid);

            }, CancellationToken.None, TaskCreationOptions.None, scheduler);

                
            Task.WaitAll(t1, t2);
                
        
        }

       
        public static void PQuickSortTP<T>(T[] vals, int min, int max, int depth)
            where T : IComparable<T> {
            int first = min, last = max;
            if (min >= max) return;
            if ((max - min) < Threshold) {
                QuickSort(vals, min, max);
                return;
            }


            T pivot = vals[(min + max) / 2];
            Partition(vals, pivot, ref min, ref max);

            if (depth <= MaxDepth) {
                CountdownEvent cde = new CountdownEvent(2);
                ThreadPool.QueueUserWorkItem(
                    _ => {
                        //int tid = Thread.CurrentThread.ManagedThreadId;
                        //Console.WriteLine("-->Sort({0},{1}, #{2:00})", first, max, tid);
                        Interlocked.Increment(ref totaltasks);
                        ThreadReport.RegistReport();
                        PQuickSortTP(vals, first, max, depth + 1);
                        //Console.WriteLine("<--Sort({0},{1}, #{2:00})", first, max, tid);
                        cde.Signal();
                    });

                ThreadPool.QueueUserWorkItem(
                    _ => {
                        //int tid = Thread.CurrentThread.ManagedThreadId;
                        //Console.WriteLine("-->Sort({0},{1}, #{2:00})", min, last, tid);
                        Interlocked.Increment(ref totaltasks);
                        ThreadReport.RegistReport();
                        PQuickSortTP(vals, min, last, depth + 1);
                        cde.Signal();
                        //Console.WriteLine("<--Sort({0},{1}, #{2:00})", min, last, tid);
                    });


                cde.Wait();
            }
            else {
               
                PQuickSortTP(vals, first, max, MaxDepth + 1);
                PQuickSortTP(vals, min, last, MaxDepth + 1);
            }

            
        }

        
        private static void CheckSorted<T>(T[] vals) where T : IComparable<T> {
            for (int i = 0; i < ArraySize - 1; ++i)
                if (vals[i].CompareTo(vals[i + 1]) > 0)
                    throw new Exception("Not Sorted!");
        }

        private static void ShowVals<T>(T[] vals) {
            for (int i = 0; i < ArraySize; ++i)
                Console.WriteLine(vals[i]);
        }

        public static void PQuickSortTest() {
            int[] vals = CreateVals();

            QuickSort(vals, 0, ArraySize - 1); // Warm up


            while (true) {
                vals = CreateVals();
                Stopwatch sw = Stopwatch.StartNew();
                long elapsedTime;
                QuickSort(vals, 0, ArraySize - 1);
                elapsedTime = sw.ElapsedMilliseconds;
                CheckSorted(vals);
                Console.WriteLine("Sequential Sort done in {0}ms!", elapsedTime);

                vals = CreateVals();
                sw.Restart();
                totaltasks = 0;

                PQuickSort(vals, 0, ArraySize - 1, 0);

                elapsedTime = sw.ElapsedMilliseconds;
                CheckSorted(vals);
                Console.WriteLine("Parallel Sort done in {0}ms!", elapsedTime);
                ThreadReport.ShowStats();
                ThreadReport.ResetAll();
                Console.WriteLine("Total tasks = {0}\n", totaltasks);
            }

        
        }
    }
}
