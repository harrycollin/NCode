

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace NCode
{
    [System.Serializable]
    [System.Obsolete]
    public class NPacketContainer
    {
        volatile MemoryStream ms;
        volatile BinaryReader reader;
        volatile BinaryWriter writer;

        public Packet packetid;
        public byte[] packet;
        public int length;
        public byte[] packetData;
        public int Start;

        
        public void RestartStream()
        {
            position = 0;
            GetPacketInfo();
        }

        public int position
        {
            get
            {
                return (int)ms.Position;
            }
            set
            {
                ms.Seek(value, SeekOrigin.Begin);
            }
        }
    
        void Reset()
        {
            try
            {
                ms = null;
                reader = null;
                writer = null;
                ms = new MemoryStream();
                reader = new BinaryReader(ms);
                writer = new BinaryWriter(ms);
            }
            catch (Exception e)
            {
                Tools.Print("@Buffer.Flush", Tools.MessageType.error, e);
            }          
        }

        /// <summary>
        /// Resets the memory streams and assigns the BinaryReader/Writer to the memory stream.
        /// </summary>
        /// <param name="bytes"></param>
        public void Create(byte[] bytes)
        {
            packet = bytes;
            Reset();
            ms = new MemoryStream(bytes);
            reader = new BinaryReader(ms);
            writer = new BinaryWriter(ms);
        }

        /// <summary>
        /// Begins reading the packet. Reads the PacketLength etc. 
        /// </summary>
        /// <returns></returns>
        public BinaryReader BeginReading()
        {
            GetPacketInfo();
            return reader;
        }

        public BinaryWriter BeginWriting(Packet packet)
        {
            Reset();
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(0);
            writer.Write((byte)packet);
            return writer;
        }

        /// <summary>
        /// Ends the writing process and returns a byte[]  of the memory stream. Use when writing to this container.
        /// </summary>
        /// <returns></returns>
        public byte[] EndWriting()
        {
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write((int)ms.Length - 4);
            length = (int)ms.Length - 4;
            packet = ms.ToArray();
            position = 0;
            //Tools.Print((ms.Length - 4).ToString());      
            return ms.ToArray();
        }

        /// <summary>
        /// Simply retuns the memory stream as a byte[]. Used when no writing is needed and you are simply aiming to forward the packet.
        /// </summary>
        /// <returns></returns>
        public byte[] End()
        {
            return packet;
        }

        public void GetPacketInfo()
        {
            length = reader.ReadInt32();
            packetid = (Packet)reader.ReadByte();
            packetData = reader.ReadBytes(length);
         
            //Tools.Print(PacketLength.ToString());
            //Tools.Print(packetid.ToString());
            Create(packetData);
        }
    }
    
}
