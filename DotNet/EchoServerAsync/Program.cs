using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tracker;

#pragma warning disable 420

//
// A Tcp multithreaded echo server, using synchronous I/O.
//

static class TcpEchoServerIOAsync {

    private const int SERVER_PORT = 8888;
    private const int BUFFER_SIZE = 1024;

    private const int MIN_SERVICE_TIME = 100;
    private const int MAX_SERVICE_TIME = 1000;

    //
    // Thread local random.
    //

    private static ThreadLocal<Random> tlr =
                new ThreadLocal<Random>(() => new Random(Thread.CurrentThread.ManagedThreadId));

    //
    // Processes the connection represented by the specified TcpClient socket.
    //

    private static int requestCount = 0;

    /* This is an asynchronous method */
    static async Task ProcessConnectionAsync(TcpClient conn, Logger logger) {

        NetworkStream s = null;
        try {
            logger.LogMessage(string.Format(
                "ProcessConnection processed in thread {0}",
                Thread.CurrentThread.ManagedThreadId));
            //
            // Get a stream for reading and writing through the socket.
            //

            s = conn.GetStream();
            byte[] buf = new byte[BUFFER_SIZE];

            //
            // Receive the request (less than 1024 bytes);
            //

            int bytesRead = await s.ReadAsync(buf, 0, buf.Length);

            //
            // Convert the request to ASCII and display it.
            //

            string request = Encoding.ASCII.GetString(buf, 0, bytesRead);
            logger.LogMessage(string.Format("Request: {0}", request));
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

            await s.WriteAsync(responseBuffer, 0, responseBuffer.Length);

            //
            // Increment the number od requests served.
            //

            Interlocked.Increment(ref requestCount);
        }
        catch (Exception ex) {
            logger.LogMessage(string.Format("***Socket exception: {0}", ex.Message));
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
    private const int MAX_ACTIVE_CONNECTIONS = 10;

    /// <summary>
    /// This version creates "a priori" a pool of accepts
    /// This is bad since it is not scalable 
    /// </summary>
    /// <param name="srv"></param>
    /// <param name="logger"></param>
    public static void ListenAsync0(TcpListener srv, Logger logger) {
        AsyncCallback cont = null;
        cont = (ar) => {
            try {
                TcpClient client = srv.EndAcceptTcpClient(ar);
               
                logger.LogMessage(String.Format("Listener - Connection established with {0}.",
                    client.Client.RemoteEndPoint));

                ProcessConnectionAsync(client, logger).ContinueWith(_ => {
                    srv.BeginAcceptTcpClient(cont, null);
                });

            }
            catch (Exception exc) {
                logger.LogMessage(string.Format("***exception {0}: {1} , stack trace--> {2}", 
                    exc.GetType().Name, exc.Message, exc.StackTrace));
            }
        };
        // create a priori a pool of MAX_ACTIVE_CONNECTIONS accepts
        for (int i = 0; i < MAX_ACTIVE_CONNECTIONS; ++i) {
            srv.BeginAcceptTcpClient(cont, null);
        }

    }

    /// <summary>
    /// This is probably the simpler version but it is not efficent and scalable
    /// since it is based on operation Task.WhenAny that as o O(N) Cost
    /// </summary>
    /// <param name="server"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    static async Task ListenAsync1(TcpListener server, Logger logger) {

        var ts = new HashSet<Task>();
        do {
            try {
                var client = await server.AcceptTcpClientAsync();

                ts.Add(ProcessConnectionAsync(client, logger));

                //
                // If the threshold was reached, wait until one of the active
                // worker threads complete its processing.
                //

                if (ts.Count >= MAX_ACTIVE_CONNECTIONS) {
                    ts.Remove(await Task.WhenAny(ts));
                }
            }
            catch (SocketException exc) {
                logger.LogMessage(string.Format("***socket exception: {0}", exc.Message));
            }
           
        } while (true);
     
    }

    /// <summary>
    /// This is probably the simpler version but it is not efficent and scalable
    /// since it is based on operation Task.WhenAny that as o O(N) Cost
    /// </summary>
    /// <param name="server"></param>
    /// <param name="logger"></param>
    /// <returns></returns>
    static async Task ListenAsync2(TcpListener server, Logger logger) {
        var sem = new SemaphoreSlim(MAX_ACTIVE_CONNECTIONS);
 
        do {
            try {
                await sem.WaitAsync();
                var client = await server.AcceptTcpClientAsync();

                ProcessConnectionAsync(client, logger).ContinueWith(_ => {
                    //sem.Release();
                });

                //
                // If the threshold was reached, wait until one of the active
                // worker threads complete its processing.
                //

                
            }
            catch (SocketException exc) {
                logger.LogMessage(string.Format("***socket exception: {0}", exc.Message));
            }

        } while (true);

    }

    /// <summary>
    /// This is the msot efficient version
    /// A counter is mantained to 
    /// </summary>
    /// <param name="server"></param>
    /// <param name="logger"></param>
    static void ListenAsyncOk(TcpListener server, Logger logger) {
        int activeConnections = 0;
        Action<Task<TcpClient>> cont = null;
       
        cont = (ant) => {
            try {
                TcpClient client = ant.Result;

                int currActive = Interlocked.Increment(ref activeConnections);
                if (currActive < MAX_ACTIVE_CONNECTIONS) {
                    server.AcceptTcpClientAsync().ContinueWith(cont);
                }
                ProcessConnectionAsync(client, logger).ContinueWith((ant2) => {
                    if (Interlocked.Decrement(ref activeConnections) == MAX_ACTIVE_CONNECTIONS - 1) {
                        server.AcceptTcpClientAsync().ContinueWith(cont);
                    }
                   
                }, TaskContinuationOptions.ExecuteSynchronously);
            }
            catch (SocketException sockex) {
                logger.LogMessage(string.Format("***socket exception: {0}", sockex.Message));
            }
        };

        server.AcceptTcpClientAsync().ContinueWith(cont);   
    }
   

    static void Main() {
        TcpListener server = null;

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


        //
        // Socket Listen as an async operation
        //
        ListenAsync2(server, logger);

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

        Console.WriteLine("--- processed requests: {0}", requestCount);

        Thread.Sleep(10000);
        // What Happens to current active connections?
    }
}
