using System;
using System.IO;
using System.Threading;

namespace Aula_2017_11_30 {
    public class AsyncUtils {

        // Cópia assíncrona de streams ao estilo APM
        public static IAsyncResult BeginCopyStream(Stream dest, Stream src,
            AsyncCallback cb, object state) {
            var gar = new GenericAsyncResult<int>(cb, state, false);
            byte[] buffer = new byte[4096];
            int totalBytes = 0;
            AsyncCallback cb_global = null;
            cb_global = ar => {
                if (ar != null) {
                    Console.WriteLine("EndWrite called in thread {0}, CompletedSynchronously is {1}",
                        Thread.CurrentThread.ManagedThreadId, ar.CompletedSynchronously);
                    dest.EndWrite(ar);
                }
                src.BeginRead(buffer, 0, 4096, ar2 => {
                    Console.WriteLine("EndRead called n thread {0}, CompletedSynchronously is {1}",
                     Thread.CurrentThread.ManagedThreadId, ar2.CompletedSynchronously);
                    int nr = src.EndRead(ar2);
                    if (nr == 0) {
                        gar.SetResult(totalBytes);
                        return;
                    }
                    totalBytes += nr;
                    dest.BeginWrite(buffer, 0, nr, cb_global, null);
                }, null);
            };
            cb_global(null);
            return gar;

        }

        public static int EndCopyStream(IAsyncResult ar) {
            return ((GenericAsyncResult<int>)ar).Result;
        }
    }
}
