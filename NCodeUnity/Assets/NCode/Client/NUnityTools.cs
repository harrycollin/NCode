using UnityEngine;
using System.Collections;
using System;
namespace NCode.Utilities
{

    public class NUnityTools
    {

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
    }
}
