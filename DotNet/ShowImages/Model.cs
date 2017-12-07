using Aula_2017_11_30;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
 


namespace ShowImages {
    class Model {

        public static Image GetFromFile(string path) {
            Thread.Sleep(3000);
            return Image.FromFile(path);
        }

        public static Task<int> CopyStreamAsync(Stream dst, Stream src) {
            var proxyTask = new TaskCompletionSource<int>();
            AsyncUtils.BeginCopyStream(dst, src, (ar) => {
                int r = AsyncUtils.EndCopyStream(ar);
                proxyTask.SetResult( r);
            }, null);
            return proxyTask.Task;
        }

        public static Task<int> CopyStream2Async(Stream dst, Stream src) {
            var proxyTask = new TaskCompletionSource<int>();
            byte[] buffer = new byte[4096];
            int totalBytes = 0;
            Action<Task> cont = null;
            cont = t => {
                src.ReadAsync(buffer, 0, 4096).
                ContinueWith(ant => {
                    int nr = ant.Result;
                    if (nr == 0)
                        proxyTask.SetResult(totalBytes);
                    else {
                        totalBytes += nr;
                        dst.WriteAsync(buffer, 0, 4096).ContinueWith(cont);
                    }
                });
            };
            cont(null);
            return proxyTask.Task;
        }

        public static Task<Image> GetFromFileAsync(string path) {
            MemoryStream ms = new MemoryStream();
            FileStream fs = new FileStream(path, FileMode.Open);
            byte[] buffer = new byte[4096];
            Task t = Task.Factory.FromAsync(
                AsyncUtils.BeginCopyStream, AsyncUtils.EndCopyStream, ms, fs, null);
            
            return t.ContinueWith(_ => {
                return Image.FromStream(ms);
            });  
        }

        // APM version of asynchronous GetImageFromFile
        IAsyncResult BeginGetImageFromFile(string path,
                AsyncCallback cb, object state) {
            var gar = new GenericAsyncResult<Image>(cb, state, false);
            MemoryStream ms = new MemoryStream();
            FileStream fs = new FileStream(path, FileMode.Open);

            AsyncUtils.BeginCopyStream(ms, fs, (ar) => {
                int r = AsyncUtils.EndCopyStream(ar);
                gar.SetResult(Image.FromStream(ms));
            }, null);
            return gar;
        }

        Image EndGetImageFromFile(IAsyncResult ar) {
            return ((GenericAsyncResult<Image>)ar).Result;
        }



    }
}
