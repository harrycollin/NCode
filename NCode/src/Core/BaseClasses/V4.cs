using System;

namespace NCode.Core.BaseClasses
{
    [Serializable]
    public struct V4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public V4(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public static V4 Zero()
        {
            return new V4(0, 0, 0, 0);
        }

        public static bool operator == (V4 value1, V4 value2)
        {
            return value1.X == value2.X
                   && value1.Y == value2.Y
                   && value1.Z == value2.Z
                   && value1.W == value2.W;
        }

        public static bool operator !=(V4 value1, V4 value2)
        {
            return !(value1 == value2);
        }
    }
}