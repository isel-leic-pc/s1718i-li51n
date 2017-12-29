using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aula_2017_11_30;

namespace V2015_2016 {

     

    class Exec {
        public interface Services<S, R> {
            Uri PingServer(Uri server);
            R ExecService(Uri server, S service);
        }
        public R ExecOnNearServer<S, R>(Services<S, R> svc, Uri[] servers, S service) {
            return default(R);
        }
    }

    class APMExec {
        public interface APMServices<S, R> {
            IAsyncResult BeginPingServer(Uri server, AsyncCallback cb, object state);
            Uri EndPingServer(IAsyncResult ar);

            IAsyncResult BeginExecService(Uri server, S service, AsyncCallback cb, object state);
            R EndExecService(IAsyncResult ar);
        }

        public IAsyncResult BeginExecOnNearServer<S, R>(APMServices<S, R> svc, Uri[] servers, 
                                S service, AsyncCallback cb, object state) {
            var gar = new GenericAsyncResult<R>();
            int count = 0;
            foreach (Uri uri in servers) {
                svc.BeginPingServer(uri, (ar) => {
                    Uri u = svc.EndPingServer(ar);
                    if (Interlocked.Increment(ref count) == 1) {
                           svc.BeginExecService(u, service, (ar2) => {
                               gar.SetResult(svc.EndExecService(ar2));
                           },null);
                    }
                },null);    
            }
            return gar;
        }
        // Now with exception handling 
        public IAsyncResult BeginExecOnNearServerE<S, R>(APMServices<S, R> svc, Uri[] servers,
                                S service, AsyncCallback cb, object state) {
            var gar = new GenericAsyncResult<R>();
            int count = 0, failures = 0;
            foreach (Uri uri in servers) {
                svc.BeginPingServer(uri, (ar) => {
                    try {
                        Uri u = svc.EndPingServer(ar);
                        if (Interlocked.Increment(ref count) == 1) {
                            svc.BeginExecService(u, service, (ar2) => {
                                try {
                                    gar.SetResult(svc.EndExecService(ar2));
                                }
                                catch (Exception e) {
                                    gar.SetException(e);
                                }

                            }, null);
                        }
                    }
                    catch (Exception e) {
                        if (Interlocked.Increment(ref failures) == servers.Length) {
                            gar.TrySetException(e);
                        }
                    }
                }, null);
            }
            return gar;
        }

        public R EndExecOnNearServer<R>(IAsyncResult ar) {
            return ((GenericAsyncResult<R>)ar).Result ;
        }
    }

    class TAPExec {
        public interface TAPServices<S, R> {
            Task<Uri> PingServerAsync(Uri server);
            
            Task<R> ExecServiceAsync(Uri server, S service);
          
        }

        public static Task<R> ExecOnNearServerAsync<S, R>(TAPServices<S, R> svc, Uri[] servers,
                                S service ) {
            var proxyTask = new TaskCompletionSource<R>();
            int count = 0;
            foreach (Uri uri in servers) {
                svc.PingServerAsync(uri).ContinueWith(ant => {
                    Uri u = ant.Result;
                    if (Interlocked.Increment(ref count) == 1) {
                        svc.ExecServiceAsync(u, service).ContinueWith(ant2 => {
                            proxyTask.SetResult(ant2.Result);
                        });
                    }
                });   
            }
            return proxyTask.Task;

        }

        // with Task.WhenAny and exception handling
        public static async Task<R> ExecOnNearServer2Async<S, R>(TAPServices<S, R> svc, Uri[] servers,
                             S service) {
            LinkedList<Task<Uri>> pingTasks = new LinkedList<Task<Uri>>(); 
            
            foreach (Uri uri in servers) {
                pingTasks.AddLast(svc.PingServerAsync(uri));
                    
            }
            Task <Uri> t = null;
          
            do {
              
                t = await Task.WhenAny(pingTasks);
                if (!t.IsFaulted) break;
                   
                pingTasks.Remove(t);
                
            }
            while (pingTasks.Count > 0);
            if (t.IsFaulted)
                throw new Exception("Ping Error", t.Exception.InnerException);
            return await svc.ExecServiceAsync(t.Result, service);
         
        }

    }
}
