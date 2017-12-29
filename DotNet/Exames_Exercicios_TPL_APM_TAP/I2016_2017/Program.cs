using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I2016_2017 {
    class Program {
        static void Main(string[] args) {
            var services = new TAPExecute.TAPServicesIMPL();
            var td = TAPExecute.Run2Async(services);

            Console.WriteLine("D result = {0}", td.Result);
        }
    }
}
