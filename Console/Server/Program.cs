using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        // Incoming data from the client
        public static string data = null;

        public static string TruncateLeft(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string LocalIPAddress()
        {
            IPHostEntry host;
            string localIP = "";
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }

        public static void StartListening()
        {
            Socket listener = null;
            Socket handler = null;

            // Data buffer from incoming data
            byte[] bytes = new byte[1024];

            // Establish the local endpoint for the socket
            // Dns.GetHostName returns the name of the
            // host running the application
            IPAddress localIPAddress = IPAddress.Parse(LocalIPAddress());
            IPEndPoint localEndPoint = new IPEndPoint(localIPAddress, 11000);

            // Create a TCP/IP Socket
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and
            // listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);

                // Start listening for connections.
                while (true)
                {
                    Console.WriteLine("Waiting for connections.....");

                    // Program is suspended while waiting for an incoming connection.
                    handler = listener.Accept();
                    data = null;

                    // An incoming connection needs to be processed.
                    while (true)
                    {
                        bytes = new byte[1024];
                        int bytesRec = handler.Receive(bytes);
                        data += Encoding.UTF8.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<eof>") > -1)
                            break;
                    }
                    // Truncate the <EOF>
                    data = TruncateLeft(data, data.Length - 5);

                    // Show the data on the console
                    Console.WriteLine("Text received : {0}", data);

                    // Echo the data back to the client
                    data = "[Server Echo 메시지]" + data;
                    byte[] msg = Encoding.UTF8.GetBytes(data);

                    handler.Send(msg);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("Socket 에러 : {0}", se.ToString());
                switch (se.SocketErrorCode)
                {
                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionReset:
                        handler.Close();
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            StartListening();
            Console.ReadLine();
        }
    }
}
