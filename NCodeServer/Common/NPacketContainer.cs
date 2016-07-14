

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace NCode
{
    [System.Serializable]
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

        public void Flush()
        {
            try
            {
                ms = null;
                reader = null;
                writer = null;
            }
            catch (Exception e)
            {
                Tools.Print("@Buffer.Flush", Tools.MessageType.error, e);
            }
        }

        public void Reset()
        {
            Flush();
            ms = new MemoryStream();
            reader = new BinaryReader(ms);
            writer = new BinaryWriter(ms);
        }

        /// <summary>
        /// Resets the memory streams and assigns the BinaryReader/Writer to the memory stream.
        /// </summary>
        /// <param name="bytes"></param>
        public void Create(byte[] bytes)
        {
            packet = bytes;
            Flush();
            ms = new MemoryStream(bytes);
            reader = new BinaryReader(ms);
            writer = new BinaryWriter(ms);
        }

        /// <summary>
        /// Begins reading the packet. Reads the length etc. 
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
         
            //Tools.Print(length.ToString());
            //Tools.Print(packet.ToString());
            Create(packetData);
        }
    }
    
}
