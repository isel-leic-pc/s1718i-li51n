using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace V2015_2016
{
    class UnsafeCountDownLatch {
        private static readonly int WAIT_GRAIN = 10;
        private int count;
        
        public UnsafeCountDownLatch(int initial) { 
            if (initial > 0) count = initial; 
        }
        
        public void signal() { //throws IllegalStateException  
            if (count > 0) count--;
            else throw new InvalidOperationException();
        }

        public bool await(long timeout) {//throws IllegalStateException  
            if (timeout < 0) timeout = long.MaxValue;
            for (; count > 0 && timeout > 0; timeout -= WAIT_GRAIN) 
                Thread.Sleep(WAIT_GRAIN);
            return count == 0;      
        }
    }

    class SafeCountDownLatch {
        private static readonly int WAIT_GRAIN = 10;
        private volatile int count;
        
        public SafeCountDownLatch(int initial) { 
            if (initial > 0) count = initial; 
        }
        
        public void signal() { //throws IllegalStateException  
            int obs;
            do  {
                obs=count;
                if (obs == 0) throw new InvalidOperationException();
            }
            while(Interlocked.CompareExchange(ref count, obs-1,obs) != obs);  
        }

        public bool await(long timeout)   { //throws IllegalStateException  
            if (timeout < 0) timeout = long.MaxValue;
            for (; count > 0 && timeout > 0; timeout -= WAIT_GRAIN)
                Thread.Sleep(WAIT_GRAIN);
            return count == 0;
        }
    }
}
