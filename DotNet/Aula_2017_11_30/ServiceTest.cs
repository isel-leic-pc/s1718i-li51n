using System;
using System.Threading.Tasks;

namespace Aula_2017_11_30 {
    class ServiceTest {
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
    }
}
