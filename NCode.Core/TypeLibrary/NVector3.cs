﻿using System;

namespace NCode.Core.TypeLibrary
{
    [Serializable]
    public struct NVector3
    {
        public bool Equals(NVector3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is NVector3 && Equals((NVector3) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                return hashCode;
            }
        }

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
            return value1.Equals(value2);
        }

        public static bool operator !=(NVector3 value1, NVector3 value2)
        {
            return !value1.Equals(value2);
        }
    }
}