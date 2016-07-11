using NCode.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if UNITY_EDITOR || UNITY_STANDALONE 
using UnityEngine;
#endif
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



#if UNITY_EDITOR || UNITY_STANDALONE
        static public object[] ReadObjectArrayEx(this BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len < 0) return null;
            List<object> objsList = new List<object>();
            for(int i = 0; i < len; i++)
            {
               objsList.Add(reader.ReadObject());
            }
            Tools.Print(len.ToString());

            return objsList.ToArray();

        }
         static public void WriteObjectArrayEx(this BinaryWriter writer, object[] b)
        {
            if (b == null)
            {
                writer.Write(-1);
            }
            else
            {
                int len = b.Length;
                writer.Write(len);
                if (len < 0) return;
                for(int i = 0; i < len; i++)
                {
                    writer.WriteObject(b[i]);
                    
                }
            }

        }

        static void WriteObject(this BinaryWriter writer, object obj)
        {
            Type type = obj.GetType();
            int prefix = GetPrefix(type);
            if (prefix == 0) return;
            writer.Write(prefix);
            switch (prefix)
            {
                case 1: writer.Write((Vector3)obj); break;
                case 2: writer.Write((Vector2)obj); break;
                case 3: writer.Write((Quaternion)obj); break;
                    
            }
        }

        static object ReadObject(this BinaryReader reader)
        {
            Type type = GetType(reader.ReadInt32());
            int prefix = GetPrefix(type);
            if (prefix == 0) return null;
            switch (prefix)
            {          
                case 1: return reader.ReadVector3(); 
                case 2: return reader.ReadVector2();
                case 3: return reader.ReadQuaternion();

            }
            return null;
        }

        static int GetPrefix(Type type)
        {
            if (type == typeof(Vector3)) return 1;
            if (type == typeof(Vector2)) return 2;
            if (type == typeof(Quaternion)) return 3;
            return 0;
        }

        static Type GetType(int prefix)
        {
            switch (prefix)
            {
                case 1: return typeof(Vector3);
                case 2: return typeof(Vector2);
                case 3: return typeof(Quaternion);
            }
            return null;
        }

        //--------------------------- Write extensions ---------------------------// 

        public static void Write(this BinaryWriter writer, Vector2 v)
        {
            if (v != null)
            {
                writer.Write(v.x);
                writer.Write(v.y);
            }
        }

        public static void Write(this BinaryWriter writer, Vector3 v)
        {
            if (v != null)
            {
                writer.Write(v.x);
                writer.Write(v.y);
                writer.Write(v.z);
            }
        }
        
    
        static void Write(this BinaryWriter writer, Quaternion v)
        {
            if (v != null)
            {
                writer.Write(v.x);
                writer.Write(v.y);
                writer.Write(v.z);
                writer.Write(v.w);
            }
        }

        //--------------------------- Read extensions ---------------------------// 
        
        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            if (float.IsNaN(x)) x = 0f;
            if (float.IsNaN(y)) y = 0f;
            return new Vector2(x, y);
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            if (float.IsNaN(x)) x = 0f;
            if (float.IsNaN(y)) y = 0f;
            if (float.IsNaN(z)) z = 0f;
            return new Vector3(x, y, z);
        }

        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();
            if (float.IsNaN(x)) x = 0f;
            if (float.IsNaN(y)) y = 0f;
            if (float.IsNaN(z)) z = 0f;
            if (float.IsNaN(w)) w = 0f;
            return new Quaternion(x, y, z, w);
        }
#endif
    }
}

