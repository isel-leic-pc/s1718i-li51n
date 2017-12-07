using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aula_2017_12_04 {
    public class NewTaskUtils {

        public static Task<int> FromAsync(
            Func<byte[], int, int, AsyncCallback, object, IAsyncResult> beginAsync,
            Func<IAsyncResult, int> endAsync, byte[] buffer, int offset, int count, object state) {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();


            beginAsync(buffer, offset, count, (ar) => {
                try {

                    tcs.TrySetResult(endAsync(ar));
                }
                catch (Exception e) {
                    tcs.TrySetException(e);
                }
            }, state);
            return tcs.Task;
        }
    }
}
