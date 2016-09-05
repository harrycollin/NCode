using NCode.Core;
using NCode.Utilities;
using System;
using System.IO;

#if UNITY_EDITOR || UNITY_STANDALONE 
using UnityEngine;
#endif
namespace NCode.Utilities
{
    /// <summary>
    /// A useful class for reading & writing extensions for BinaryReader & Writer.
    /// Aims to completely eliminate all complicated read/writer operations. Read\WriteObject will eventually handle all needed types
    /// </summary>
    public static class BinaryExtensions
    {
        /// <summary>
        /// Writes a byte array. Has to be used with ReadByteArray
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="b"></param>
        static void WriteByteArray(this BinaryWriter writer, byte[] b)
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

        /// <summary>
        /// Reads a byte array. Has to be used with WriteByteArray 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        static byte[] ReadByteArray(this BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len > 0) return reader.ReadBytes(len);
            if (len < 0) return null;
            return new byte[0];
        }

        /// <summary>
        /// Reads an object array. Use with WriteObjectArray.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static object[] ReadObjectArrayEx(this BinaryReader reader)
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

        /// <summary>
        /// Writes an object array
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="b"></param>
        public static void WriteObjectArrayEx(this BinaryWriter writer, object[] b)
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

        /// <summary>
        /// Writes a single object 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="obj"></param>
        public static void WriteObject(this BinaryWriter writer, object obj)
        {
            Type type = obj.GetType();
            int prefix = GetPrefix(type);
            if (prefix == 0) return;
            writer.Write(prefix);
            switch (prefix)
            {
                case 99: writer.Write((NetworkObject)obj); break;
                case 1: writer.Write((Guid)obj); break;
#if UNITY_EDITOR || UNITY_STANDALONE
                case 2: writer.Write((Vector2)obj); break;
                case 3: writer.Write((Vector3)obj); break;
                case 4: writer.Write((Vector4)obj); break;
                case 5: writer.Write((Quaternion)obj); break;
#endif
                case 60: writer.Write((NPlayer)obj); break;
            }
        }

        /// <summary>
        /// Reads a single object
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static object ReadObject(this BinaryReader reader)
        {
            Type type = GetType(reader.ReadInt32());
            int prefix = GetPrefix(type);
            if (prefix == 0) return null;
            switch (prefix)
            {
                case 99: return reader.ReadNetworkObject();
                case 1: return reader.ReadGUID();
#if UNITY_EDITOR || UNITY_STANDALONE
                case 2: return reader.ReadVector2(); 
                case 3: return reader.ReadVector3();
                case 4: return reader.ReadVector4();
                case 5: return reader.ReadQuaternion();
#endif
                case 50: return reader.ReadNPlayer();
            }
            return null;
        }

        /// <summary>
        /// Gets the prefix depending on the types
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static int GetPrefix(Type type)
        {
            if (type == typeof(NetworkObject)) return 99;
            if (type == typeof(Guid)) return 1;
#if UNITY_EDITOR || UNITY_STANDALONE
            if (type == typeof(Vector2)) return 2;
            if (type == typeof(Vector3)) return 3;
            if (type == typeof(Vector4)) return 4;
            if (type == typeof(Quaternion)) return 5;

#endif
            if (type == typeof(NPlayer)) return 60;
            return 0;
        }

        /// <summary>
        /// Gets the type depending on the prefix
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        static Type GetType(int prefix)
        {
            switch (prefix)
            {
                case 99: return typeof(NetworkObject);
                case 1: return typeof(Guid);
#if UNITY_EDITOR || UNITY_STANDALONE
                case 2: return typeof(Vector2);
                case 3: return typeof(Vector3);
                case 4: return typeof(Vector4);
                case 5: return typeof(Quaternion);
#endif
                case 60: return typeof(NPlayer);
            }
            return null;
        }

        //--------------------------- Write extensions ---------------------------// 
#if UNITY_EDITOR || UNITY_STANDALONE

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
        public static void Write(this BinaryWriter writer, Vector4 v)
        {
            if (v != null)
            {
                writer.Write(v.x);
                writer.Write(v.y);
                writer.Write(v.z);
                writer.Write(v.w);
            }
        }


        public static void Write(this BinaryWriter writer, Quaternion v)
        {
            if (v != null)
            {
                writer.Write(v.x);
                writer.Write(v.y);
                writer.Write(v.z);
                writer.Write(v.w);
            }

        }
        

#endif  // -------- Unity Dependant Above ------// 
        public static void Write(this BinaryWriter writer, NetworkObject o)
        {
            if (o != null)
            {
                writer.WriteByteArray(NConverters.ConvertObjectToByteArray(o));
            }
        }
        public static void Write(this BinaryWriter writer, Guid v)
        {
            if (v != null)
            {
                writer.WriteByteArray(NConverters.ConvertObjectToByteArray(v));
            }
        }
        public static void Write(this BinaryWriter writer, NPlayer player)
        {
            if(player != null)
            {
                writer.WriteByteArray(NConverters.ConvertObjectToByteArray(player));
            }
        }

        //--------------------------- Read extensions ---------------------------// 

#if UNITY_EDITOR || UNITY_STANDALONE

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

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            float z = reader.ReadSingle();
            float w = reader.ReadSingle();
            if (float.IsNaN(x)) x = 0f;
            if (float.IsNaN(y)) y = 0f;
            if (float.IsNaN(z)) z = 0f;
            if (float.IsNaN(w)) w = 0f;
            return new Vector4(x, y, z, w);
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
        public static NetworkObject ReadNetworkObject(this BinaryReader reader)
        {
            NetworkObject o = (NetworkObject)NConverters.ConvertByteArrayToObject(reader.ReadByteArray());
            if (o == null) return null;
            return o;
        }
        public static Guid ReadGUID(this BinaryReader reader)
        {
            Guid guid = (Guid)NConverters.ConvertByteArrayToObject(reader.ReadByteArray());
            if (guid != null) return guid;
            Guid empty = Guid.Empty;
            return empty;
        }
        public static NPlayer ReadNPlayer(this BinaryReader reader)
        {
            NPlayer player = (NPlayer)NConverters.ConvertByteArrayToObject(reader.ReadByteArray());
            if (player != null) return player;
            return null;
        }
    }
}

