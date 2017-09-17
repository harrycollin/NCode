using System;

namespace NCode.Core.TypeLibrary
{
    [Serializable]
    public struct NVector4
    {
        public bool Equals(NVector4 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is NVector4 && Equals((NVector4) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ W.GetHashCode();
                return hashCode;
            }
        }

        public float X;
        public float Y;
        public float Z;
        public float W;


        public NVector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static NVector3 Zero()
        {
            return new NVector3(0, 0, 0);
        }

        public static bool operator == (NVector4 value1, NVector4 value2)
        {
            return value1.Equals(value2);
        }

        public static bool operator !=(NVector4 value1, NVector4 value2)
        {
            return !value1.Equals(value2);
        }
    }
}