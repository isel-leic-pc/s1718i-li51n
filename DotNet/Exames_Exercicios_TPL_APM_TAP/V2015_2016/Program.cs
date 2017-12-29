using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace V2015_2016
{
    class MyTAPServices : TAPExec.TAPServices<string, string> {
        public Task<string> ExecServiceAsync(Uri server, string service) {
            return Task.FromResult(server.AbsoluteUri);
        }

        public Task<Uri> PingServerAsync(Uri server) {
            if (server.Host == "x") {
                return Task.Run(() => {
                    Thread.Sleep(4000);
                   
                    return Task.FromResult(server);
                });
               
            }
            else {
                return Task.Run(() => {
                    Thread.Sleep(2000);
                    //throw new Exception("Bad Host name y");
                    return default(Uri);
                });
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Uri[] servers = { new Uri("http://y/teste"), new Uri("http://x/teste") };
            try {
                var tr = TAPExec.ExecOnNearServer2Async(new MyTAPServices(),
                    servers, "teste");
                Console.WriteLine(tr.Result);
            }
            catch(Exception e) {
                Console.WriteLine(e);
            }

        }
}
}
