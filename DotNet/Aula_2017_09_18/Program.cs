using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Aula_2017_09_18
{
    class Program
    {
        private static void WasteCPU(int wasteGrade)
        {
            for (int i = 0; i < wasteGrade; ++i)
            {
                long sum = 0;
                for (int j = 0; j < 100000000; ++j)
                    sum += i;
            }
        }
        private static void ThreadFunc()
        {
            Console.Write("Thread {0}, ", 
                Thread.CurrentThread.ManagedThreadId);
            if (Thread.CurrentThread.IsBackground)
                Console.WriteLine("background thread");
            else
                Console.WriteLine("foreground thread");
            Thread.Sleep(10000);
        }
        /// <summary>
        /// 
        /// Evaluate proc termination behaviour
        /// funtion of background thread state
        /// </summary>
        /// <param name="background"></param>
        public static void BackgroundThreadTest(bool background)
        {
        
            // Create 1 thread for test
            Thread t1 = new Thread(ThreadFunc);
            
            // define if is or not a background thread
            t1.IsBackground = background;
            
            // Put thread on ready state
            t1.Start();
            
            // return immediately
        }

        public static void ThreadInterruption()
        {
            Thread t = new Thread(() => {
                WasteCPU(4);
                Console.WriteLine("After Wait CPU");
                try
                {
                    Console.WriteLine("Enter sleep");
                    Thread.Sleep(10000);
                }
                catch (ThreadInterruptedException)
                {

                }

            });
            t.Start();
            t.Interrupt();
            //uncomment next two lines and explain behaviour
            for (int i = 0; i < 100000; ++i)
                Console.Write("Main Thread Message {0}\r", i);
            t.Join();
        }


        static void Main(string[] args)
        {
            //Uncomment to check thread interruption behaviour
            ThreadInterruption();


            // uncomment on of next calls to check different behaviour 
            // being new thread background or foreground
            // true means created thread is a background thread
            
            //BackgroundThreadTest(true);
            //BackgroundThreadTest(false);
        }  
    }
}
