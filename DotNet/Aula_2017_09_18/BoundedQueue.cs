using System;
using System.Collections.Generic;
using System.Threading;


namespace Aula_2017_09_18
{
    class BoundedQueue<T> : IDisposable
    {
        private LinkedList<T> items;
        private int maxItems;
        private Mutex mutex;
        private Semaphore hasSpace, hasItems;

        public BoundedQueue(int maxItems)
        {
            items = new LinkedList<T>();
            this.maxItems = maxItems;
            mutex = new Mutex();
            hasSpace = new Semaphore(maxItems, maxItems);
            hasItems = new Semaphore(0, maxItems);
        }

        public void Put(T item)
        {
            hasSpace.WaitOne();
            mutex.WaitOne();
            items.AddLast(item);
            mutex.ReleaseMutex();
            hasItems.Release();
        }

        public T Get()
        {
            hasItems.WaitOne();
            mutex.WaitOne();
            T item = items.First.Value;
            items.RemoveFirst();
            mutex.ReleaseMutex();
            hasSpace.Release();
            return item;
        }

        public void Dispose()
        {
            mutex.Dispose();
            hasSpace.Dispose();
            hasItems.Dispose();
        }
    }
}
