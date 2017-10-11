 

namespace Aula_2017_09_18.Test
{
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.Threading;

    [TestFixture]
    public class BoundQueueTests
    {
        
        [Test]
        public void CheckPutBlockingOnAFullBoundedQueue()
        {

            var bq = new BoundedQueue<int>(1);
            bool interrupted = false;

            Thread t = new Thread(() =>
            {
                try {
                    bq.Put(1);
                    // will block
                    bq.Put(2);
                }
                catch(ThreadInterruptedException)
                {
                    interrupted = true;
                }
            });

            t.Start();
            // Give a security margin to thread t1 execution.
            // Not really rocket science, but...
            Thread.Sleep(3000);

            t.Interrupt();
            t.Join();
            // success if the previous interruption
            // removes thread from blocking in second Put operation
            Assert.IsTrue(interrupted);
        }

         
        [Test]
        public void TestTransfer1To10ViaBoundedQueue()
        {
            int[] sendingNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            int[] receivedNumbers = new int[10];
            using (var bb = new BoundedQueue<int>(1))
            {
                Thread producer = new Thread(() =>
                {
                    foreach(int i in sendingNumbers)
                           bb.Put(i);              
                });

                Thread consumer = new Thread(() =>
                {    
                    for(int i= 0; i < 10; ++i) {
                        int val = bb.Get();
                        receivedNumbers[i] = val;
                    }
                });

                consumer.Start();
                producer.Start();
                producer.Join();
                consumer.Join();
                Assert.AreEqual(sendingNumbers, receivedNumbers);
            }
        }  
    }
}
