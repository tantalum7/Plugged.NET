#region Imports
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
#endregion

namespace Plugged.NET
{
    public class UdpListener : IDisposable
    {
        #region Fields
        public int port;
        public bool isRunning { get { return _isRunning(); } }
        private Thread listenerThread;
        public ConcurrentQueue<byte[]> listenerQueue;
        private int killFlag = 0;
        #endregion

        #region Public Methods
        public UdpListener(int port)
        {
            // Store port in class
            this.port = port;

            // Initialise queue
            listenerQueue = new ConcurrentQueue<byte[]>();

            // Instantiate listener thread
            listenerThread = new Thread(new ThreadStart(ListenerThreadFunc));
            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        ~UdpListener()
        {
            // Send abort to the listenThread
            Dispose();
        }

        /// <summary>
        /// Retrieves the most recent packet from the queue, or null
        /// </summary>
        public byte[] TryGetLatestPacket()
        {
            byte[] packet;
            return listenerQueue.TryDequeue(out packet) ? packet : null;
        }

        /// <summary>
        /// Sends UDP packet to target IP/Port
        /// </summary>
        public void SendPacket(IEnumerable<byte> packetBytes, IPEndPoint remoteEP, int repeatCount=1)
        {

            UdpClient txClient = new UdpClient();
            for (int i = 0; i < repeatCount; i++) { txClient.Send(packetBytes.ToArray(), packetBytes.Count(), remoteEP); }
            txClient.Close();
        }

        public void SendPacket(IEnumerable<byte> packetBytes, string remoteIP, int remotePort, int repeatCount=1)
        {
            SendPacket(packetBytes, new IPEndPoint(IPAddress.Parse(remoteIP), remotePort), repeatCount);
        }

        /// <summary>
        /// IDisposable Dispose() method. Called at the end of 'using' block
        /// </summary>
        public void Dispose()
        {
            Interlocked.Increment(ref killFlag);
        }
        #endregion

        #region Private Methods
        private bool _isRunning()
        {
            if (listenerThread != null) { return listenerThread.IsAlive; }
            else { return false; }
        }

        private void ListenerThreadFunc()
        {
            // Instantiate new UdpClient
            UdpClient client = new UdpClient(port);
            client.Client.ReceiveTimeout = 1000;

            // Keep loop running until killFlag is > 0
            while (Interlocked.Exchange(ref killFlag, killFlag) == 0)
            {
                // Wrap receive code around try block to catch exceptions
                try
                {
                    IPEndPoint clientEP = new IPEndPoint(IPAddress.Any, port);
                    byte[] data = client.Receive(ref clientEP);

                    // Push the packet text onto the queue
                    listenerQueue.Enqueue(data);
                }

                // Catch and ignore SocketExceptions (from timeouts)
                catch (SocketException)
                { ; }
            }

            // Close the client 
            client.Close();
        }
        #endregion
    }
}
