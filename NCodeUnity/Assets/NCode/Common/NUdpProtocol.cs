using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
namespace NCode
{
    public class NUdpProtocol
    {
        static public IPAddress defaultNetworkInterface = null;

        // Port used to listen and socket used to send and receive
        int mPort = -1;
        Socket thisSocket;

        // Buffer used for receiving incoming data
        byte[] tempBuffer = new byte[8192];
        NBuffer tempPacket;

        // End point of where the data is coming from
        EndPoint mEndPoint;

        // Default end point -- mEndPoint is reset to this value after every receive operation.
        static EndPoint mDefaultEndPoint;

        // Incoming message queue
        protected Queue<NUdpDatagram> mIn = new Queue<NUdpDatagram>();
        protected Queue<NUdpDatagram> mOut = new Queue<NUdpDatagram>();

        /// <summary>
        /// Whether we can send or receive through the UDP socket.
        /// </summary>
        public bool isActive { get { return mPort != -1; } }

        /// <summary>
        /// Port used for listening.
        /// </summary>
        public int listeningPort { get { return mPort > 0 ? mPort : 0; } }


        /// <summary>
        /// Start listening for incoming messages on the specified port.
        /// </summary>
        public bool Start(int port)
        {
            Stop();
            mPort = port;
            thisSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            try
            {
                thisSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            }
            catch (System.Exception) { }

            // Port zero means we will be able to send, but not receive
            if (mPort == 0) return true;

            try
            {
                IPAddress networkInterface = defaultNetworkInterface ?? IPAddress.Any;
                mEndPoint = new IPEndPoint(networkInterface, 0);
                mDefaultEndPoint = new IPEndPoint(networkInterface, 0);

                // Bind the socket to the specific network interface and start listening for incoming packets
                thisSocket.Bind(new IPEndPoint(networkInterface, mPort));
                
                thisSocket.BeginReceiveFrom(tempBuffer, 0, tempBuffer.Length, SocketFlags.None, ref mEndPoint, OnReceive, null);
            }

            catch (System.Exception ex) { Tools.Print("Udp.Start: " + ex.Message); Stop(); return false; }
            
            return true;
        }


        /// <summary>
        /// Stop listening for incoming packets.
        /// </summary>

        public void Stop()
        {
            mPort = -1;

            if (thisSocket != null)
            {
                thisSocket.Close();
                thisSocket = null;
            }
        }

        /// <summary>
        /// Receive incoming data.
        /// </summary>

        void OnReceive(IAsyncResult result)
        {
            Tools.Print("UDP Receive");
            if (!isActive) return;
            int bytes = 0;

            try
            {
                bytes = thisSocket.EndReceiveFrom(result, ref mEndPoint);
            }
            catch (System.Exception ex)
            {

            }

            if (bytes > 4)
            {
                // This datagram is now ready to be processed
                NBuffer buffer = new NBuffer();
                buffer.InitialiseWithData(tempBuffer);

                // The 'endPoint', gets reassigned rather than updated.
                NUdpDatagram dg = new NUdpDatagram();
                dg.container = buffer;
                dg.ip = (IPEndPoint)mEndPoint;
                lock (mIn) mIn.Enqueue(dg);
            }

            // Queue up the next receive operation
            if (thisSocket != null)
            {
                mEndPoint = mDefaultEndPoint;

                try
                {
                    thisSocket.BeginReceiveFrom(tempBuffer, 0, tempBuffer.Length, SocketFlags.None, ref mEndPoint, OnReceive, null);
                }
                catch (System.Exception e)
                {
                    
                }
            }
        }

        /// <summary>
        /// Extract the next packet in the 'inQueue' is there is any.
        /// </summary>
        public bool ReceivePacket(out NBuffer buffer, out IPEndPoint source)
        {

            if (mPort == 0)
            {
                Stop();
                throw new System.InvalidOperationException("You must specify a non-zero port to UdpProtocol.Start() before you can receive data.");
            }
            else if (mIn.Count != 0)
            {
                lock (mIn)
                {
                    NUdpDatagram dg = mIn.Dequeue();
                    buffer = dg.container;
                    source = dg.ip;
                    return true;
                }
            }
            buffer = null;
            source = null;
            return false;
        }

        

        /// <summary>
        /// Send the specified datagram.
        /// </summary>

        public void Send(NBuffer buffer, IPEndPoint ip)
        {
            Tools.Print(ip.ToString());
            if (thisSocket != null)
            {
                Tools.Print(ip.ToString() + " not null");

                lock (mOut)
                {
                    Tools.Print(ip.ToString() + " lock");

                    NUdpDatagram dg = new NUdpDatagram();
                    dg.container = buffer;
                    dg.ip = ip;
                    mOut.Enqueue(dg);


                    if (mOut.Count > -1)
                    {
                        try
                        {
                            Tools.Print(buffer.length + " + " + buffer.position);
                            buffer.position = 0;
                            // If it's the first datagram, begin the sending process
                            thisSocket.BeginSendTo(buffer.EntirePacket, buffer.position, buffer.length, SocketFlags.None, ip, OnSend, null);
                            Tools.Print(ip.ToString() + " sent");

                        }
                        catch (Exception ex)
                        {
                            Tools.Print("Error on sending udp", Tools.MessageType.error, ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Send completion callback. Recycles the datagram.
        /// </summary>

        void OnSend(IAsyncResult result)
        {
            Tools.Print("hhhvhdfvudfsvbd");
            if (!isActive) return;
            int bytes = 0;

            try
            {
                bytes = thisSocket.EndSendTo(result);
            }
            catch (System.Exception ex)
            {
                bytes = 1;

            }

            lock (mOut)
            {
                //mOut.Dequeue().container;

                if (bytes > 0 && thisSocket != null && mOut.Count != 0)
                {
                    // If there is another packet to send out, let's send it
                    NUdpDatagram dg = mOut.Peek();
                    thisSocket.BeginSendTo(dg.container.EntirePacket, dg.container.position, dg.container.length, SocketFlags.None, dg.ip, OnSend, null);
                }
            }
            Tools.Print("Send via udp");

        }

        public BinaryWriter BeginSend(Packet packet)
        {
            tempPacket = null;
            tempPacket = new NBuffer();
            return tempPacket.BeginWriting(packet);
        }     
        
        public void EndSend(IPEndPoint IP)
        {
            tempPacket.EndWriting();
            Send(tempPacket, IP);
        } 
    }
}

