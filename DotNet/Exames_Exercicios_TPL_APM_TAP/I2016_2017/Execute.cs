using System;
using System.Threading;
using System.Threading.Tasks;
using Aula_2017_11_30;

namespace I2016_2017 {
    class Execute {
        public interface Services {
            A Oper1();
            B Oper2(A a);
            C Oper3(A a);
            D Oper4(B b, C c);
        }
        public static D Run(Services svc) {
            var a = svc.Oper1();
            return svc.Oper4(svc.Oper2(a), svc.Oper3(a));
        }
    }
     
   
    public class A {}
    public class B { }
    public class C { }
    public class D { }

    public class APMExecute {
        
        public interface APMServices {
            IAsyncResult BeginOper1(AsyncCallback cb, object state);
            A EndOper1(IAsyncResult ar);
            IAsyncResult BeginOper2(A a,AsyncCallback cb, object state);
            B EndOper2(IAsyncResult a);
            IAsyncResult BeginOper3(A a,AsyncCallback cb, object state);
            C EndOper3(IAsyncResult a);
            IAsyncResult BeginOper4(B b, C c,AsyncCallback cb, object state);
            D EndOper4(IAsyncResult ar);
        }
        
        public static IAsyncResult BeginRun(APMServices svc, 
            AsyncCallback cb, object st) {
            GenericAsyncResult<D> gar = new GenericAsyncResult<D>();
            int count = 2;
            B b=null;
            C c=null;
            AsyncCallback callback_2_3  = (ar)=> {
                try {
                    int oper =(int) ar.AsyncState;
                    if (oper == 2) {
                        Volatile.Write(ref b, svc.EndOper2(ar));
                    }
                    else {
                        Volatile.Write(ref c, svc.EndOper3(ar)); 
                    }
                    if (Interlocked.Decrement(ref count) == 0) {
                        B lb = Volatile.Read(ref b);
                        C lc = Volatile.Read(ref c);
                        svc.BeginOper4(lb, lc, (ar4) => {
                            try {
                                D d = svc.EndOper4(ar4);
                                gar.TrySetResult(d);
                            }
                            catch(Exception e) {
                                gar.TrySetException(e);
                            }
                        },null);
                    }
                }
                catch(Exception e) {
                        gar.TrySetException(e);
                }
            };
            svc.BeginOper1(ar=>  {
                try {
                    A a = svc.EndOper1(ar);
                    // Start both oper 2 and oper3 operations
                    // and continue them using the same callback
                    svc.BeginOper2(a, callback_2_3, 2);
                    svc.BeginOper3(a, callback_2_3, 3);
                }
                catch(Exception e) {
                    gar.TrySetException(e);
                }
            }, null);
            return gar;
        }

        public static D EndRun(IAsyncResult ar) {
            return ((GenericAsyncResult<D>)ar).Result;
        }
    }

    public class TAPExecute {
        public interface TAPServices {
            Task<A> Oper1Async();
           
            Task<B> Oper2Async(A a);
           
            Task<C> Oper3Async(A a);
            
            Task<D> Oper4Async(B b, C c);
             
        }

        public class TAPServicesIMPL : TAPServices {
            public Task<A> Oper1Async() {
                return Task.Run(async () => {
                    await Task.Delay(1000);
                    return new A();
                });
               
            }

            public  Task<B> Oper2Async(A a) {
                return Task.Run(async () => {
                    await Task.Delay(1000);
                    return new B();
                });
            }

            public  Task<C> Oper3Async(A a) {
                return Task.Run(async () => {
                    await Task.Delay(1000);
                    return new C();
                });
            }

            public  Task<D> Oper4Async(B b, C c) {
                 return Task.Run(async () => {
                    await Task.Delay(1000);
                    return new D();
                });
            }
        }
           

        public static async Task<D> RunAsync(TAPServices svc) {
            var a = await svc.Oper1Async();
            
            var t1 = svc.Oper2Async(a);
            var t2 = svc.Oper3Async(a);
            await Task.WhenAll(t1, t2);
           
            return await svc.Oper4Async(t1.Result, t2.Result);
             
        }

        public static  Task<D> Run2Async(TAPServices svc) {
            var promise = new TaskCompletionSource<D>();
            var toper1 = svc.Oper1Async();
            var tres = toper1.ContinueWith(ant => {
                Console.WriteLine("After toper1 conclusion!");
                var toper2 = svc.Oper2Async(toper1.Result);
                var toper3= svc.Oper3Async(toper1.Result);
                Console.WriteLine("After toper1 and toper2 start!");
                return Task.WhenAll(toper2, toper3).ContinueWith(ant2 => {
                    svc.Oper4Async(toper2.Result, toper3.Result).ContinueWith((ant3 => {
                        promise.SetResult(ant3.Result);
                    }));
                });
            });
            return promise.Task;
        }

        public static Task<D> Run3Async(TAPServices svc) {
            var toper1 = svc.Oper1Async();
            var tres = toper1.ContinueWith(ant => {
                Console.WriteLine("After toper1 conclusion!");
                var toper2 = svc.Oper2Async(toper1.Result);
                var toper3 = svc.Oper3Async(toper1.Result);
                Console.WriteLine("After toper1 and toper2 start!");
                return Task.WhenAll(toper2, toper3).
                    ContinueWith(ant2 => {
                        return svc.Oper4Async(toper2.Result, toper3.Result);
                    });
                 
            });
            return tres.Unwrap().Unwrap();
            
        } 
    }
}
