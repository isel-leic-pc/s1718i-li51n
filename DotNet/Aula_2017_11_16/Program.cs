using System;
using System.Threading;

namespace Aula_2017_11_16 {

    public class AsyncInvokeTest {

        static bool isPrime(int n) {
            throw new IndexOutOfRangeException();
            Console.WriteLine("IsPrime thread:  {0}, fromThreadPool={1}",
                Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
            if (n == 2) return true;
            if (n < 2 || n % 2 == 0) return false;
            for (int i = 3; i <= Math.Sqrt(n); i += 2)
                if (n % i == 0) return false;
            return true;
        }

        public static bool CalculateIsPrime(int i) {

            Console.WriteLine("Main thread:  {0}, fromThreadPool={1}",
              Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
            Func<int, bool> pFunc = isPrime;

            IAsyncResult ar = pFunc.BeginInvoke(i, null, null);

            // return result, waiting while it has not been produced
           
            return pFunc.EndInvoke(ar);
        }
    }
    class Program {

        static void Main(string[] args) {
            try {
                bool p = AsyncInvokeTest.CalculateIsPrime(23);
                Console.WriteLine(p);
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }   
        }
    }
}
