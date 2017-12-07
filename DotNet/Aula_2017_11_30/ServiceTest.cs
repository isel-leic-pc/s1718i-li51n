using System;
using System.Threading.Tasks;

namespace Aula_2017_11_30 {
    public static class ServiceTest {
        public interface Service {
            int FindUserId(String name, String bdate);
            Uri ObtainAvatarUri(int userId);  
        }

        public interface APM_Service {
            IAsyncResult BeginFinduserId(String name, String bdate,
                AsyncCallback cb, object state);
            int EndFindUserId(IAsyncResult ar);

            IAsyncResult BeginObtainAvatarUri(int userId,
                AsyncCallback cb, object state);

            Uri EndObtainAvatarUri(IAsyncResult ar);

        }

        public interface TAP_Service {
            Task<int> FindUserIdAsync(String name, String bdate);
            Task<Uri> ObtainAvatarUriAsync(int userId);
        }

        public static Uri GetUserAvatar(Service svc, String name, String bdate) {
            int userId = svc.FindUserId(name, bdate);
            return svc.ObtainAvatarUri (userId); 
        }

        IAsyncResult BeginGetUserAvatar(APM_Service svc, String name, String bdate,
            AsyncCallback cb, object state) {
            var gar = new GenericAsyncResult<Uri>(cb, state, false);
            
            svc.BeginFinduserId(name, bdate, (ar) => {
                int id = svc.EndFindUserId(ar);
                svc.BeginObtainAvatarUri(id, ar2 => {
                    Uri uri = svc.EndObtainAvatarUri(ar2);
                    gar.SetResult(uri);
                }, null);
            }, null);
            return gar;
        }

        Uri EndGetUserAvatar(IAsyncResult ar) {
            return ((GenericAsyncResult<Uri>)ar).Result;
        }

        public static Task<T> MyUnwrap<T>(this Task<Task<T>> task) {
            var proxyTask = new TaskCompletionSource<T>();

            task.ContinueWith(ant1 => {
                if (ant1.IsFaulted) {
                    proxyTask.TrySetException(ant1.Exception.InnerException);
                }
                else if (ant1.IsCanceled) {
                    proxyTask.TrySetCanceled();
                }
                else {
                    var tr = ant1.Result;
                    tr.ContinueWith(ant2 => {
                        if (ant2.IsFaulted) {
                            proxyTask.TrySetException(ant2.Exception.InnerException);
                        }
                        else if (ant2.IsCanceled) {
                            proxyTask.TrySetCanceled();
                        }
                        else {
                            proxyTask.SetResult(ant2.Result);
                        }
                    });
                     
                }
            });
            return proxyTask.Task;

        }

        public static Task<Uri> GetUserAvatarAsync(TAP_Service svc, String name, String bdate) {
            var t = svc.FindUserIdAsync(name, bdate).
             ContinueWith(ant => {
                 return svc.ObtainAvatarUriAsync(ant.Result);
             }).MyUnwrap();
            return t;
        }

    }
}
