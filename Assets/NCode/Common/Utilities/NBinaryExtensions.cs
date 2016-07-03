using NCode.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace NCode
{
    public static class BinaryExtensions
    {
        static public void WriteByteArrayEx(this BinaryWriter writer, byte[] b)
        {
            if (b == null)
            {
                writer.Write(-1);
            }
            else
            {
                int len = b.Length;
                writer.Write(len);
                if (len > 0) writer.Write(b);
            }

        }
        static public byte[] ReadByteArrayEx(this BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len > 0) return reader.ReadBytes(len);
            if (len < 0) return null;
            return new byte[0];
        }

    }
}

