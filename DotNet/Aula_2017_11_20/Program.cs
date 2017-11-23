using System;
using System.Threading;
using System.Threading.Tasks;

 

namespace Aula_2017_11_20 {
    class Program {
        public static bool IsPrime(int p) {
            if (p < 0)
                throw new InvalidOperationException("Invalid number");
            Console.WriteLine("IsPrime thread:  {0}, fromThreadPool={1}",
                Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
            if (p == 2) return true;
            if (p < 2 || p % 2 == 0) return false;
            for (int i = 3; i <= Math.Sqrt(p); i += 2)
                if (p % i == 0) return false;
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

        public static Task<bool> IsPrimeAsync(int p) {
            Task<bool> t = new Task<bool>(() => {
                return IsPrime(p);
            });
            t.Start();
            return t;
        }

        public static bool AreAllPrimes(params int[] numbers) {
            var tasks = new Task<bool>[numbers.Length];

            for(int i=0; i < numbers.Length; ++i) {
                tasks[i] = IsPrimeAsync(numbers[i]);
            }

            var tall = Task.Factory.ContinueWhenAll<bool>(tasks, (ts) => {
                foreach(Task<bool> t in ts) {
                    if (t.IsFaulted)
                        throw new Exception("Error on task!");
                    else if (t.Result == false) return false;
                }
                return true;

            },TaskContinuationOptions.ExecuteSynchronously);

            tall.Wait();

            return tall.Result;
      
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

       

        static void Main(string[] args) {
            Console.WriteLine(AreAllPrimes(7, 11, 13));
        }
    }
}
