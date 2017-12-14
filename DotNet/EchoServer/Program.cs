using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Tracker;

#pragma warning disable 420

//
// A Tcp multithreaded echo server, using synchronous I/O.
//

static class TcpEchoServerIOSync {

    private const int SERVER_PORT = 8888;
    private const int BUFFER_SIZE = 1024;

    private const int MIN_SERVICE_TIME = 1000;
    private const int MAX_SERVICE_TIME = 5000;

    //
    // Thread local random.
    //

    private static ThreadLocal<Random> tlr =
                new ThreadLocal<Random>(() => new Random(Thread.CurrentThread.ManagedThreadId));

    //
    // Processes the connection represented by the specified TcpClient socket.
    //

    private volatile static int requestCount = 0;

    /* This is an asynchronous method */
    static void ProcessConnection(TcpClient conn, Logger logger) {

        NetworkStream s = null;
        try {

            //
            // Get a stream for reading and writing through the socket.
            //

            s = conn.GetStream();
            byte[] buf = new byte[BUFFER_SIZE];

            //
            // Receive the request (less than 1024 bytes);
            //

            int bytesRead = s.Read(buf, 0, buf.Length);

            //
            // Convert the request to ASCII and display it.
            //

            string request = Encoding.ASCII.GetString(buf, 0, bytesRead);
            //Console.WriteLine("Request: {0}", request);

            //
            // Simulate a random service time 
            //
            Thread.Sleep(tlr.Value.Next(MIN_SERVICE_TIME, MAX_SERVICE_TIME));

            string response = request.ToUpper();

            //
            // Send the response
            //

            byte[] responseBuffer = Encoding.ASCII.GetBytes(response);

            //
            // Await because of the try-catch-finally block.
            //

            s.Write(responseBuffer, 0, responseBuffer.Length);

            //
            // Increment the number od requests served.
            //

            Interlocked.Increment(ref requestCount);
        }
        catch (Exception ex) {
            Console.WriteLine("***Socket exception: {1}", ex.Message);
        }
        finally {
            if (s != null) {
                s.Close();
            }
            conn.Close();
        }
    }


    //
    // listen for connections.
    //

    static void Listen(TcpListener server, Logger logger) {

        do {
            try {
                var client = server.AcceptTcpClient();
                logger.LogMessage(string.Format("Listener - Connection established with {0}, {1} in I/O ThreadPool thread.",
                            client.Client.RemoteEndPoint,
                            Thread.CurrentThread.IsThreadPoolThread ? "" : "not"));

                ThreadPool.QueueUserWorkItem(_ => ProcessConnection(client, logger));
            }
            catch (SocketException sockex) {
                logger.LogMessage(string.Format("***socket exception: {0}", sockex.Message));
            }

        } while (true);


    }

    static void Main() {
        TcpListener server = null;

        var cts = new CancellationTokenSource();
        //
        // Create a listen socket bind to the server port.
        //
      
        server = new TcpListener(IPAddress.Parse("127.0.0.1"), SERVER_PORT);

        //
        // Start listening for client requests.
        //

        server.Start();

        Logger logger = new Logger();
        logger.Start();

        logger.LogMessage(string.Format(
              "Main thread is thread {0}",
              Thread.CurrentThread.ManagedThreadId));
        //
        // Socket Listen in a dedicated thread
        //
        new Thread(_ => {
            Listen(server, logger);

        }).Start();
       
        //
        // Wait a <enter> from the console to terminate the server. 
        //

        Console.WriteLine("Hit <enter> to exit the server...");
        Console.ReadLine();

        //
        // Stop listening.
        //

        server.Stop();
        logger.Stop();

        cts.Cancel();

      
        Console.WriteLine("--- processed requests: {0}", requestCount);
        Console.ReadLine();
        // What Happens to current active connections?
    }
}
