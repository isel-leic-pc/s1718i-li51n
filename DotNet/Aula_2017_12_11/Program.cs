using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Aula_2017_12_07;

namespace Aula_2017_12_11 {
    class Program {

        public static void ShowHeaders(HttpClient request) {
            foreach (var header in request.DefaultRequestHeaders) {
                Console.WriteLine("header.Key= {0}, header.Value ={1}", header.Key, header.Value);

            }
        }
        public static async Task TestOrderByCompletion() {
            string[] sites = {
                "https://www.isel.pt" ,
                "https://www.rtp.pt" ,
                "https://www.microsoft.com"
            };
            var tasks = new List<Task<string>>();

            foreach (string site in sites) {
                Console.WriteLine("Get {0}", site);
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0");
               
                tasks.Add(client.GetStringAsync(site));
            }

            foreach (var downloadTask in tasks.OrderByCompletion()) {
                var download = await downloadTask;
                Console.WriteLine(download);
            }
        }
        static void Main(string[] args) {
            Task t = TestOrderByCompletion();
            Console.WriteLine("After TestOrderByCompletion!");
            t.Wait();
            Console.WriteLine("Done!");
        }
    }
}
