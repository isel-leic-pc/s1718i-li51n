using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

//
// Tcp client for a echo server.
//

class TcpEchoClient {
    private const int SERVER_PORT = 8888;
    private const int BUFFER_SIZE = 1024;

    private volatile static int requestCount = 0;

    //
    // Send a server request and display the response.
    //	
    static bool SendRequestAndReceiveResponse(string server, string requestMessage) {
        NetworkStream s = null;
        TcpClient connection = null;

        try {

            //
            // Create a TcpClient socket connected to the server and
            // get the associated stream.
            //

            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            connection = new TcpClient();
            connection.Connect(server, SERVER_PORT);
            s = connection.GetStream();

            //
            // Translate the message to a byte stream and send it to the server.
            //

            byte[] requestBuffer = Encoding.ASCII.GetBytes(requestMessage);
            s.Write(requestBuffer, 0, requestBuffer.Length);


            Console.WriteLine("Sent: {0}", requestMessage);

            //
            // Receive the server's response and display.
            //

            byte[] responseBuffer = new byte[BUFFER_SIZE];
            int bytesRead = s.Read(responseBuffer, 0, responseBuffer.Length);
            sw.Stop();
            Console.WriteLine("Received: {0} [{1} ms]",
                               Encoding.ASCII.GetString(responseBuffer, 0, bytesRead),
                               sw.ElapsedMilliseconds);
            Interlocked.Increment(ref requestCount);
            return true;
        }
        catch (Exception ) {
            //Console.WriteLine("***Exception:[{0}] {1}", requestMessage, ex.Message);
            return false;
        }
        finally {

            //
            // Close everything.
            //

            if (s != null) {
                s.Close();
            }
            if (connection != null) {
                connection.Close();
             }
        }
    }

    //
    // Send continuously batch of requests until a key is pressed.
    //

    private const int REQ_BATCH_COUNT = 1000;
    private const int MAX_DEGREE_OF_PARALLELISM = 4;


    static void Main(string[] args) {
        Stopwatch sw = Stopwatch.StartNew();
        string request = (args.Length > 0) ? args[0] : "This is the request message";
        ParallelOptions po = new ParallelOptions { MaxDegreeOfParallelism = MAX_DEGREE_OF_PARALLELISM };
        do {
            
            Parallel.For(0, REQ_BATCH_COUNT,  po, (i,s) => {
                if (!SendRequestAndReceiveResponse("localhost", String.Format("{0} #{1}", request, i)))
                    s.Stop();
            });
        } while (!Console.KeyAvailable);

        Console.WriteLine("--completed requests: {0} / {1} ms",
                             requestCount, sw.ElapsedMilliseconds);
    }

    
}
