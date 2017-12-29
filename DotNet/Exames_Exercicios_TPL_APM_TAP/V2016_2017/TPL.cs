using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace V2016_2017 {
    class TPL {
        public static T SearchItem<T>(IEnumerable<T> items, Predicate<T> query, int timeout) {
            var cts = new CancellationTokenSource();
            var monitor = new object();
            ParallelOptions options = new ParallelOptions { CancellationToken = cts.Token };
            Task t1 = Task.Delay(timeout).
                    ContinueWith(_ => cts.Cancel(), TaskContinuationOptions.ExecuteSynchronously);
          
            T result = default(T);
            try {
                Parallel.ForEach(
                    items,
                    options,

                    (t, s) => {
                        if (query(t)) {
                            lock(monitor) result = t;
                            s.Stop();
                        }
                        // just for test
                        Thread.Sleep(1000);
                    });
             
            }
            catch(OperationCanceledException ) {
                   if (result.Equals(default(T))) 
                       throw new TimeoutException();
                  
            }
            return result;
            
        }
    }
}
