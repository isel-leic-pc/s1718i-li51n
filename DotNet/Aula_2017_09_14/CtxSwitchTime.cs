using System;
using System.Threading;
using System.Diagnostics;

namespace Aula_2017_09_14
{
    class CtxSwitchTime
    {
        private static readonly int N_ITERS = 1000000;
        public void Test()
        {
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
            Thread t1 = new Thread(() =>
            {
                for (int i = 0; i < N_ITERS; ++i)
                    Thread.Yield();
            });

            Thread t2 = new Thread(() =>
            {
                for (int i = 0; i < N_ITERS; ++i)
                    Thread.Yield();
            });

           
            Stopwatch sw = Stopwatch.StartNew();
            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            sw.Stop();

            long duration = sw.ElapsedMilliseconds;

            Console.WriteLine("{0} ms elapsed, ctx time = {1} ns ",
                duration, (duration * 1000000) / (N_ITERS * 2));

        }
    }
}
