using Aula_2017_11_30;
using System;
using System.Collections.Generic;
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




        public static Task<Image> GetFromFileAsync(string path) {
            MemoryStream ms = new MemoryStream();
            FileStream fs = new FileStream(path, FileMode.Open);

            Task t = fs.CopyToAsync(ms);

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
