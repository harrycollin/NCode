using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace NCode.Utilities
{
    public class NConverters
    {
        public static byte[] ConvertObjectToByteArray(object Object)
        {
            if (Object == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, Object);
            return ms.ToArray();
        }
       
        public static object ConvertByteArrayToObject(byte[] bytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(bytes, 0, bytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            System.Object obj = (System.Object)binForm.Deserialize(memStream);
            return obj;
        }
        public static void Cunt()
        {

        }
    }
}