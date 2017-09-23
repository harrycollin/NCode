using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NCode;
using NCode.Core.Utilities;

namespace NCode.Core.Protocols
{
    public class NTcpProtocol
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
        public Queue<NBuffer> InQueue = new Queue<NBuffer>();

        /// <summary>
        /// The outbound queue of packets that need to be sent back to the client. 
        /// </summary>
        public Queue<NBuffer> OutQueue = new Queue<NBuffer>();

        /// <summary>
        /// A temporary packet for extracting exsiting packets into. 
        /// </summary>
        NBuffer tempPacket;

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
        public int PingInMs;
        
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
                Tools.Print("@NTcpProtocol.Connect", Tools.MessageType.Error, e, true);
                return false;
            }
        }
        
        /// <summary>
        /// Handles timeouts when connecting
        /// </summary>
        /// <param name="obj"></param>
        private void ConnectTimeout(object obj)
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
        private void ConnectCallback(IAsyncResult result)
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

                //Set the state to verifying
                State = ConnectionState.verifying;

                //Send a verification packet
                BinaryWriter writer = BeginSend(Packet.RequestClientSetup);
                writer.Write(ProtocolVersion);
                EndSend();
           
                isTryingToConnect = false;

                //Begin listening for incoming packets
                BeginListening();
                
            }
            catch (Exception e)
            {
                Tools.Print("@NTcpProtocol:ConnectCallback", Tools.MessageType.Error, e);
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
                    Tools.Print("@NTcpProtocol:BeginListening", Tools.MessageType.Error, e, true);
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// The callback for the async receive event. 
        /// </summary>
        private void ReceiveCallback(IAsyncResult result)
        {
            //If the state of this client is 'disconnected' then don't bother. 
            if (!isSocketConnected) return;
            int bytes = 0;
            Socket socket = (Socket)result.AsyncState;

            try
            {
                if (socket != thisSocket) return;
                bytes = socket.EndReceive(result);
            }
            catch (Exception e)
            {
                Tools.Print("@NTcpProtocol:ReceiveCallback 'socket.EndReceive'", Tools.MessageType.Error, e, true);
                if (socket != thisSocket) return;                
                Disconnect();
                return;
            }

            //Set now as being the last time we received a message from this socket. 
            LastReceiveTime = DateTime.UtcNow.Ticks / 10000;

            if (bytes == 0)
            {
                Close();
            }
            else if (ProcessPacket())
            {              
                try
                {
                    if (isSocketConnected)
                    {
                        // Queue up the next receive operation
                        thisSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallback, thisSocket);
                    }
                }
                catch (Exception e)
                {
                    Tools.Print("@NTcpProtocol:ReceiveCallback 'Queue up next receive operation' ", Tools.MessageType.Error, e, true);
                    Close();
                }
            }
            else Close();
        }

        /// <summary>
        /// Processes packets from the client. (Needs to be refined along with the NPacketContainer
        /// </summary>
        private bool ProcessPacket()
        {
            if ( thisSocket != null && isSocketConnected && buffer.Length != 0)
            {
                try
                {
                    lock (InQueue)
                    {
                        if (buffer.Length != 0)
                        {
                            NBuffer packet = new NBuffer();
                            packet.Initialize(buffer);
                            InQueue.Enqueue(packet);
                            return true;
                        }
                    }
                }
                catch(Exception e)
                {
                    Tools.Print("@NTcpProtocol.ProcessPacket", Tools.MessageType.Error, e, true);
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Closes the socket instantly
        /// </summary>
        private void Close()
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
        public bool Disconnect()
        {
            if (thisSocket != null && isSocketConnected)
            {
                try
                {
                    lock (thisSocket)
                    {
                        Tools.Print(thisSocket.RemoteEndPoint.ToString() + ": disconnected", Tools.MessageType.Notification, null, true);
                        thisSocket.Shutdown(SocketShutdown.Both);
                        thisSocket.Close();
                        thisSocket = null;
                        State = ConnectionState.disconnected;
                    }
                }
                catch (Exception e)
                {
                    Tools.Print("@NTcpProtocol.Disconnect", Tools.MessageType.Error, e, true);
                }
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Grabs the next packet in the InQueue 
        /// </summary>
        public bool NextPacket(out NBuffer packet)
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
        public BinaryWriter BeginSend(Packet packet)
        {
            tempPacket = null;
            tempPacket = new NBuffer();
            return tempPacket.BeginWriting(packet);
        }

        /// <summary>
        /// End the sending process
        /// </summary>
        public void EndSend()
        {
            Send(tempPacket.EndWriting());
        }

        /// <summary>
        /// Sends the provided byte array to the remote socket
        /// </summary>
        /// <param name="data"></param>
        private void Send(byte[] data)
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
                    Tools.Print("@NTcpProtocol.Send", Tools.MessageType.Error, e, true);
                }
            }
        }

        /// <summary>
        /// The callback for the async send event. 
        /// </summary>
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
                Tools.Print("@NTcpProtocol:SendCallback", Tools.MessageType.Error, e, true);
            }
        }    

        /// <summary>
        /// Pings the remote socket. 
        /// </summary>
        public void Ping()
        {
            if(thisSocket != null && isSocketConnected)
            {
                BinaryWriter writer = BeginSend(Packet.Ping);
                EndSend();
            }
        }
         
    }
}
