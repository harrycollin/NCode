using System;

namespace NCode.Core.TypeLibrary
{
    [Serializable]
    public struct NVector3
    {
        public float X;
        public float Y;
        public float Z;

        public NVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static NVector3 Zero()
        {
            return new NVector3(0, 0, 0);
        }

        public static bool operator == (NVector3 value1, NVector3 value2)
        {
            return value1.X == value2.X
                   && value1.Y == value2.Y
                   && value1.Z == value2.Z;
        }

        public static bool operator !=(NVector3 value1, NVector3 value2)
        {
            return !(value1 == value2);
        }
    }
}