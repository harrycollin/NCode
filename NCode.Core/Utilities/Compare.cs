#if UNITY_EDITOR || UNITY_STANDALONE
using UnityEngine;
#endif

namespace NCode.Core.Utilities
{
    public class Compare
    {
        /// <summary>
        /// Compares two floats by threshold.
        /// </summary>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static bool FloatEqual(float before, float after, float threshold)
        {

            if (before == after) return false;
            if (after == 0f || after == 1f) return true;
            float diff = before - after;
            if (diff < 0f) diff = -diff;
            if (diff > threshold) return true;
            return false;
        }

        /*------ All UnityEngine dependent functions below -------*/
#if UNITY_EDITOR || UNITY_STANDALONE


        /// <summary>
        /// Used to compare two Vector3. Threshold is set for each axis using a Vector3.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static bool Vector3Equal(Vector3 v1, Vector3 v2, Vector3 threshold)
        {
            if (v1 == v2) return true;
            if(FloatEqual(v1.x, v2.x, threshold.x) && FloatEqual(v1.y, v2.y, threshold.y) && FloatEqual(v1.z, v2.z, threshold.z)) { return true;  }
            return false;
        }

        /// <summary>
        /// Used to compare two Quaternions. Threshold is set for each axis using a Quaternion.
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static bool QuaternionEqual(Quaternion v1, Quaternion v2, Quaternion threshold)
        {
            if (v1 == v2) return true;
            if (FloatEqual(v1.x, v2.x, threshold.x) && FloatEqual(v1.y, v2.y, threshold.y) && FloatEqual(v1.z, v2.z, threshold.z) && FloatEqual(v1.w, v2.w, threshold.w)) { return true; }
            return false;
        }
#endif

    }
}