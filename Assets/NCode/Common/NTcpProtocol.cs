using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NCode;

namespace NCode
{
    public class NTcpProtocol : NPlayer
    {
        /// <summary>
        /// The current version of the Protocol
        /// </summary>
        public int ProtocolVersion = 12062016;

        /// <summary>
        /// The socket for communicating with the remote player. 
        /// </summary>
        public Socket thisSocket;

        /// <summary>
        /// The IP address of the remote player.
        /// </summary>
        public IPAddress thisIP;

        /// <summary>
        /// The inbound queue of packets that have been received but not processed by the main thread. 
        /// </summary>
        public Queue<NPacketContainer> InQueue = new Queue<NPacketContainer>();

        /// <summary>
        /// The outbound queue of packets that need to be sent back to the client. 
        /// </summary>
        public Queue<NPacketContainer> OutQueue = new Queue<NPacketContainer>();

        /// <summary>
        /// A temporary packet for extracting exsiting packets into. 
        /// </summary>
        NPacketContainer tempPacket;

        /// <summary>
        /// The timeout of
        /// </summary>
        public int Timeout = 5000;

        /// <summary>
        /// A buffer for receiving packets into. 
        /// </summary>
        byte[] buffer = new byte[8192];

        /// <summary>
        /// The last time a packet was received from the client. 
        /// </summary>
        public long LastReceiveTime;

        /// <summary>
        /// The current ping of the remote client.
        /// </summary>
        public int Ping;
        
        /// <summary>
        /// Whether the socket is currently connected. 
        /// </summary>
        public bool isSocketConnected { get { return thisSocket != null && thisSocket.Connected; } }
        
        /// <summary>
        /// Whether the player is currently connected (verified aswell)
        /// </summary>
        public bool isConnected { get { return State == ConnectionState.connected; } }
        
        /// <summary>
        /// Returns bool on whether the client is trying to connect.
        /// </summary>
        public bool isTryingToConnect = false;

        /// <summary>
        /// Possible connection states
        /// </summary>
        public enum ConnectionState
        {
            disconnected,
            verifying,
            connected,
        }

        /// <summary>
        /// The current connection state
        /// </summary>
        public ConnectionState State = ConnectionState.disconnected;
      
        /// <summary>
        /// Connects to a remote endpoint
        /// </summary>
        /// <param name="RemoteEndPoint"></param>
        /// <returns></returns>
        public bool Connect(IPEndPoint RemoteEndPoint)
        {
            if (thisSocket != null) thisSocket = null;
            if (RemoteEndPoint == null) return false;
            try
            {
                isTryingToConnect = true;

                //Assign the socket
                thisSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Connect to the remote endpoint.
                IAsyncResult result = thisSocket.BeginConnect(RemoteEndPoint, new AsyncCallback(ConnectCallback), thisSocket);

                //Start a new thread detecting the timeout
                Thread TimeoutThread = new Thread(ConnectTimeout);
                TimeoutThread.Start(result);
                return true;
            }
            catch (Exception e)
            {
                Tools.Print("@NTcpProtocol.Connect", Tools.MessageType.error, e);
            }
            return false;
        }
        
        /// <summary>
        /// Handles timeouts when connecting
        /// </summary>
        /// <param name="obj"></param>
        void ConnectTimeout(object obj)
        {
            IAsyncResult result = (IAsyncResult)obj;
            if(result != null && !result.AsyncWaitHandle.WaitOne(Timeout, true))
            {
                isTryingToConnect = false;
                Close();
            }
        }

        /// <summary>
        /// The connect callback
        /// </summary>
        /// <param name="result"></param>
        public void ConnectCallback(IAsyncResult result)
        {
            if (thisSocket == null) return;
            try
            {
                // Retrieve the socket from the state object.
                thisSocket = (Socket)result.AsyncState;

                // Complete the connection.
                thisSocket.EndConnect(result);

                // Let the client know (Only available in the editor
                Tools.Print("Connected to game server: " + thisSocket.RemoteEndPoint.ToString());

                State = ConnectionState.verifying;

                isTryingToConnect = false;

                BeginListening();

                BinaryWriter writer = BeginSend(Packet.RequestClientInfo);
                writer.Write(ProtocolVersion);
                EndSend();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Begins the listening process for the socket
        /// </summary>
        /// <returns></returns>
        public bool BeginListening()
        {
            if (thisSocket != null && thisSocket.Connected)
            {
                try
                {
                    LastReceiveTime = DateTime.UtcNow.Ticks / 10000;
                    // Begin receiving the data from the remote device.
                    thisSocket.BeginReceive(buffer, 0, buffer.Length, 0, new AsyncCallback(ReceiveCallback), thisSocket);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            return false;
        }

        public void ReceiveCallback(IAsyncResult result)
        {

            if (State == ConnectionState.disconnected) return;
            int bytes = 0;
            Socket socket = (Socket)result.AsyncState;
            try
            {
                bytes = socket.EndReceive(result);
                if (socket != thisSocket) return;
            }
            catch (System.Exception ex)
            {
                if (socket != thisSocket) return;                
                Disconnect();
                return;
            }

            LastReceiveTime = DateTime.UtcNow.Ticks / 10000;

            if (bytes == 0)
            {
                Close();
            }
            else if (ProcessPacket())
            {
                if (State == ConnectionState.disconnected) return;

                try
                {
                    // Queue up the next read operation
                    thisSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, thisSocket);
                }
                catch (System.Exception ex)
                {                   
                    Close();
                }
            }
            else Close();
        }

        /// <summary>
        /// Processes packets from the client. (Needs to be refined along with the NPacketContainer
        /// </summary>
        /// <returns></returns>
        bool ProcessPacket()
        {
            if ( thisSocket != null && buffer.Length != 0)
            {
                try
                {
                    lock (InQueue)
                    {
                        if (buffer.Length != 0)
                        {
                            NPacketContainer packet = new NPacketContainer();

                            packet.Create(buffer);

                            InQueue.Enqueue(packet);
                        }
                    }
                }
                catch(Exception e)
                {
                    Tools.Print("@NTcpProtocol.ProcessPacket", Tools.MessageType.error, e);
                }
#if UNITY_EDITOR
            Tools.Print("Received");
#endif
                return true;
            }
            return false;
        }

        /// <summary>
        /// Closes the socket instantly
        /// </summary>
        void Close()
        {
            if(thisSocket != null)
            {
                thisSocket.Close();
                thisSocket = null;
            }
        }

        /// <summary>
        /// Disconnects sockets on each end of the connection. 
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            if (thisSocket != null )
            {
                try
                {
                    lock (thisSocket)
                    {
                        Tools.Print(thisSocket.RemoteEndPoint.ToString() + ": disconnected");
                        thisSocket.Shutdown(SocketShutdown.Both);
                        thisSocket.Close();
                        thisSocket = null;
                    }
                }
                catch (Exception e)
                {
                    Tools.Print("@NTcpProtocol.Disconnect", Tools.MessageType.error, e);
                }
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Grabs the next packet in the InQueue 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool NextPacket(out NPacketContainer packet)
        {
            lock (InQueue)
            {
                if (InQueue.Count > 0)
                {
                    packet = InQueue.Dequeue();
                    return packet != null;
                }
            }         
            packet = null;
            return false;
        }

        /// <summary>
        /// Begin the sending process 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public BinaryWriter BeginSend(Packet packet)
        {
            tempPacket = null;
            tempPacket = new NPacketContainer();
            return tempPacket.BeginWriting(packet);
        }

        public void SendPacketContainer(NPacketContainer packet)
        {
            Send(packet.End());         
        }

        /// <summary>
        /// End the sending process
        /// </summary>
        public void EndSend ()
        {
            Send(tempPacket.EndWriting());
        }

        /// <summary>
        /// Sends the provided byte array to the remote socket
        /// </summary>
        /// <param name="data"></param>
        void Send(byte[] data)
        {
            if (thisSocket != null && isSocketConnected)
            {
                try
                {
                    // Begin sending the data to the remote device.
                    thisSocket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), thisSocket);
                }
                catch (Exception e)
                {
                    Tools.Print("@NTcpProtocol.Send", Tools.MessageType.error, e);
                }
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);               
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }       
    }
}
