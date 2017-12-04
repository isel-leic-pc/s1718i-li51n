using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aula_2017_11_27 {
    public class TaskUtils {

        // Crialão de task para delay assíncrono
        public static Task TaskDelay(int delayMillis) {
            var tcs = new TaskCompletionSource<object>();
            Timer t = null;

            t = new Timer(_ => {
                tcs.SetResult(null);
                t.Dispose();
            }, null, delayMillis, Timeout.Infinite);
            return tcs.Task;
        }

        // Combinador que retorna uma task que completa 
        // na conclusão das tasks recebidas por parâmetro
        public static Task WhenAll(Task[] tasks) {
            int count = tasks.Length;
            var proxyTask = new TaskCompletionSource<object>();

            foreach(Task t in tasks) {
                t.ContinueWith(_ => {
                    if (Interlocked.Decrement(ref count) == 0) {
                        proxyTask.SetResult(null);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
            return proxyTask.Task;
        }

        
    }
}
