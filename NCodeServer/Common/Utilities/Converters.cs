using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
#endif

namespace NCode.Utilities
{
    public class Converters
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

        /* All UnityEngine dependent below */
#if UNITY_EDITOR || UNITY_STANDALONE



        /// <summary>
        /// Converts a string of 3 numercial values seperated by commas to a Vector3.
        /// </summary>
        /// <param name="Vector3String"></param>
        /// <returns></returns>
        public static Vector3 StringToVector3(string Vector3String)
        {
            Vector3 vector3 = new Vector3();
            char SplittingChars = '|';
            string[] stringSplit = Vector3String.Split(SplittingChars);
            try
            {
                vector3 = new Vector3(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]), float.Parse(stringSplit[2]));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return vector3;
        }

        public static string Vector3ToString(Vector3 v)
        {
            string s;
            s = v.x + "|" + v.y + "|" + v.z;
            return s;
        }

        public static string QuaternionToString(Quaternion v)
        {
            string s;
            s = v.x + "|" + v.y + "|" + v.z + "|" + v.w;
            return s;
             
        }

        /// <summary>
        /// Converts a string of 4 numerical values separated by commas to a Quaternion.
        /// </summary>
        /// <param name="QuaternionString"></param>
        /// <returns></returns>
        public static Quaternion StringToQuaternion(string QuaternionString)
        {
            Quaternion quaternion = new Quaternion();
            char SplittingChars = '|';
            string[] stringSplit = QuaternionString.Split(SplittingChars);
            try
            {
                quaternion = new Quaternion(float.Parse(stringSplit[0]), float.Parse(stringSplit[1]), float.Parse(stringSplit[2]), float.Parse(stringSplit[3]));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Debug.Log(e);
            }
            return quaternion;
        }
#endif
    }
}