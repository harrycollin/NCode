using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NCode.Core.Utilities;

namespace NCode.Core
{
    public class NBuffer
    {

        volatile MemoryStream ms;
        volatile BinaryWriter writer;
        volatile BinaryReader reader;

        bool writing = false;
        bool used = false;
        private int receivedLength;

        public Packet packet;
        public int PacketLength
        {
            get { if (used) return (int)ms.Length; else { return receivedLength; } }
            set { receivedLength = value; }
        }
        public byte[] PacketData
        {
            get
            {          
                if (!writing && ms != null && used)
                {
                    position = 5;
                    BinaryReader temp = new BinaryReader(ms);
                    byte[] data = new byte[PacketLength];
                    data = reader.ReadBytes(PacketLength);
                    return data;
                }
                else
                {
                    return null;
                }
            }
        }
        public byte[] EntirePacket
        {
            get { if (used && !writing) return ms.ToArray(); else return null; }
        }
        

        
        public int position
        {
            get { if (ms != null) return (int)ms.Position; else return 0; }
            set { ms.Seek(value, SeekOrigin.Begin); }
        }   
        
        bool Recycle()
        {
            try
            {
                ms = null;
                writer = null;
                reader = null;
                used = false;
                return true;
            }
            catch (Exception e)
            {
                Tools.Print("Exception recycling NBuffer.", Tools.MessageType.Error, e);
            }
            return false;
        }

        public void Initialize(byte[] bytes = null)
        {
            Recycle();
            if (bytes != null) {
                ms = new MemoryStream(bytes);
            } else { ms = new MemoryStream(); }
            reader = new BinaryReader(ms);
            writer = new BinaryWriter(ms);
            if(bytes != null)
            {
                PacketLength = reader.ReadInt32();
                packet = (Packet)reader.ReadByte();
            }
            used = true;
        }

        public BinaryWriter BeginWriting(Packet packettype)
        {
            Initialize();
            packet = packettype;
            writing = true;
            writer.Write(0); //writes a 4 byte signed int. Moves position from 0 - 3;
            writer.Write((byte)packet); //writes a single byte as the packet id; position is 4;
            return writer;
        }
        
        public byte[] EndWriting()
        {
            used = true;
            writing = false;
            position = 0;
            writer.Write(PacketLength);
            return ms.ToArray();
        }

        public BinaryReader BeginReading()
        {
            if(!writing && used)
            {
                return reader;
            }
            return null;
        }       
    }
}
