using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aula_2017_11_23 {
    class Program {
        public static bool IsPrime(int p) {
            if (p < 0)
                throw new InvalidOperationException("Invalid number");
            /*
            Console.WriteLine("IsPrime thread:  {0}, fromThreadPool={1}",
                Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
                */
            if (p == 2) return true;
            if (p < 2 || p % 2 == 0) return false;
            for (int i = 3; i <= Math.Sqrt(p); i += 2)
                if (p % i == 0) return false;
            return true;
        }

        public static int  NextPrime(int p) {
            if (p < 2) return 2;
            if (p % 2 == 0) p++; else p += 2;
            while (!IsPrime(p)) p += 2;
            return p;
        }


        public static bool IsPrime(int p, CancellationToken token) {
            if (p < 0)
                throw new InvalidOperationException("Invalid number");
            Console.WriteLine("IsPrime thread:  {0}, fromThreadPool={1}",
                Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
            if (p == 2) return true;
            if (p < 2 || p % 2 == 0) return false;
            for (int i = 3; i <= Math.Sqrt(p); i += 2) {
                token.ThrowIfCancellationRequested(); 
                if (p % i == 0) return false;
            }
            return true;
        }

        public static void FirstTest() {

            Task<bool> t2 = new Task<bool>((_n) => {
                int n = (int)_n;
                return IsPrime(n);
            }, 45);

            t2.Start();

            Console.WriteLine(t2.Result);
        }

        public static Task<bool> IsPrimeAsync(int p ) {
            Task<bool> t = new Task<bool>(() => {
                return IsPrime(p);
            });
            t.Start();
            return t;
        }

        public static Task<bool> IsPrimeAsync(int p, CancellationToken token) {
            Task<bool> t = new Task<bool>(() => {
                return IsPrime(p, token);
            });
            t.Start();
            return t;
        }

        public static bool AreAllPrimes(params int[] numbers) {
            var tasks = new Task<bool>[numbers.Length];

            for (int i = 0; i < numbers.Length; ++i) {
                tasks[i] = IsPrimeAsync(numbers[i]);
            }

            var tall = Task.Factory.ContinueWhenAll<bool>(tasks, (ts) => {
                foreach (Task<bool> t in ts) {
                    if (t.IsFaulted)
                        throw new Exception("Error on task!");
                    else if (t.Result == false) return false;
                }
                return true;

            }, TaskContinuationOptions.ExecuteSynchronously);

            tall.Wait();

            return tall.Result;

        }

        public static bool AreAllPrimesParallel(params int[] numbers) {
            bool result = true;
            ParallelLoopResult r =  Parallel.For(0, numbers.Length, (i,s) => {
                if (!IsPrime(numbers[i])) {
                     Volatile.Write(ref result, false);
                     s.Break();
                     
                }
                

            });
            if (r.IsCompleted)
                Console.WriteLine("For completed!");
            else {
                Console.WriteLine("For breaked in iteration {0}",
                    r.LowestBreakIteration);
            }
            return result;
        }

         
        public static int CountAllPrimesParallel(params int[] numbers) {
            int count = 0;
            ParallelLoopResult r = Parallel.For(0,
                numbers.Length,
                () => 0,
                (i,s, v) => {
                    // local aggregation
                    return IsPrime(numbers[i]) ? v + 1 : v;
                },
                (v) => {
                    Interlocked.Add(ref count, v);
                }
            );
            
            return count;
        }

        public static int CountAllPrimesWithCancellation(IEnumerable<int> numbers, 
            CancellationToken token) {
            int count = 0;
            var options = new ParallelOptions {CancellationToken = token };
            ParallelLoopResult r = Parallel.ForEach(
                numbers,
                options,
                () => 0,
                (p, s, v) => {
                    // local aggregation
                    
                    return IsPrime(p) ? v + 1 : v;
                },
                (v) => {
                    Interlocked.Add(ref count, v);
                }
            );

            return count;
        }

        public static IEnumerable<int> GenPrimeNumbers() {
            int p = 2;

            do {
                yield return p;

                p = NextPrime(p);
            }
            while (true);
            
        }

        public static bool AreAllPrimesInSequence(int n1, int n2, int n3) {
            var t1 =
                IsPrimeAsync(n1)
                .ContinueWith((t) => {
                    if (!t.Result) return Task.FromResult(false);
                    return IsPrimeAsync(n2);
                });

            // To Continue... 
            return false;
        }

        public static void CancellationTest() {
            CancellationTokenSource cts = new CancellationTokenSource();
            Task.Delay(5000).ContinueWith(_ => {
                cts.Cancel();
            });

            try {
                Console.WriteLine("Start Count!");
                int n = CountAllPrimesWithCancellation(GenPrimeNumbers().
                       TakeWhile(p => p < 10000000), cts.Token);
                Console.WriteLine("End Count with {0} primes!", n);
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        static void Main(string[] args) {
            //Console.WriteLine(AreAllPrimes(7, 11, 13));
            //Console.WriteLine(AreAllPrimesParallel(7, 4, 13));
            //SortUtils.PQuickSortTest();

            CancellationTest();

        }
    }
}
