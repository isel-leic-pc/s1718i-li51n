using System;
using System.Threading;
using System.Diagnostics;

namespace Aula_2017_09_14
{
    class CtxSwitchTime
    {
        private static readonly int N_ITERS = 1000000;

        /// <summary>
        /// code executed by test threads
        /// </summary>
        private void ThreadFunc()
        {
            for (int i = 0; i < N_ITERS; ++i)
                Thread.Yield();
        }

        public void Test()
        {
            // force same CPU (1) for test threads
            Process.GetCurrentProcess().ProcessorAffinity = 
                new IntPtr(1);

            // Create 2 threads for test
            Thread t1 = new Thread(ThreadFunc) ;
            Thread t2 = new Thread(ThreadFunc);

            // Force thread's priority to Highest
            t1.Priority = ThreadPriority.Highest;
            t2.Priority = ThreadPriority.Highest;

            Stopwatch sw = Stopwatch.StartNew();
            t1.Start();
            t2.Start();

            // Wait for thread termination
            t1.Join();
            t2.Join();

            sw.Stop();

            long duration = sw.ElapsedMilliseconds;

            Console.WriteLine("{0} ms elapsed, ctx time = {1} ns ",
                duration, (duration * 1000000) / (N_ITERS * 2));

        }
    }
}
