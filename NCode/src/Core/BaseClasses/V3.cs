using System;

namespace NCode.Core.BaseClasses
{
    [Serializable]
    public struct V3
    {
        public float X;
        public float Y;
        public float Z;

        public V3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static V3 Zero()
        {
            return new V3(0, 0, 0);
        }

        public static bool operator == (V3 value1, V3 value2)
        {
            return value1.X == value2.X
                   && value1.Y == value2.Y
                   && value1.Z == value2.Z;
        }

        public static bool operator !=(V3 value1, V3 value2)
        {
            return !(value1 == value2);
        }
    }
}