using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NCode
{
    public class NBuffer
    {
        public Packet packet;
        public byte[] PacketData
        {
            get
            {          
                if (!writing && ms != null && used)
                {
                    BinaryReader temp = new BinaryReader(ms);
                    byte[] data = new byte[length];
                    data = reader.ReadBytes(length);
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
       

       
        volatile MemoryStream ms;
        volatile BinaryWriter writer;
        volatile BinaryReader reader;
        
        bool writing = false;
        bool used = false;
        
        public int position
        {
            get { if (ms != null) return (int)ms.Position; else return 0; }
            set { ms.Seek(value, SeekOrigin.Begin); }
        }

        public int length
        {
            get { if (ms != null && !writing) return (int)ms.Length - 4; else return 0; }
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
                Tools.Print("Exception recycling NBuffer.", Tools.MessageType.error, e);
            }
            return false;
        }

        void Initialize(byte[] bytes = null)
        {
            Recycle();
            if (bytes != null) { ms = new MemoryStream(bytes); Tools.Print("NOT NULL"); } else { ms = new MemoryStream(); Tools.Print("NULL"); }
            reader = new BinaryReader(ms);
            writer = new BinaryWriter(ms);
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
            writer.Write(length);
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

        public void InitialiseWithData(byte[] bytes)
        {
            if (!writing) Initialize(bytes); else return;
            packet = (Packet)reader.ReadByte();
            used = true;
        }
    }
}
