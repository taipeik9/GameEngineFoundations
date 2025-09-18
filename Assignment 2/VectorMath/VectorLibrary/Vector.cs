using System;

namespace VectorLibrary
{
    public class Vector(float _x, float _y, float _z)
    {
        public float x = _x;
        public float y = _y;
        public float z = _z;

        // vector addition
        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        // vector subtraction
        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        // dot product
        public float Dot(Vector other)
        {
            return (x * other.x) + (y * other.y) + (z * other.z);
        }

        public Vector Cross(Vector other)
        {
            return new Vector(
                (y * other.z) - (z * other.y),
                (z * other.x) - (x * other.z),
                (x * other.y) - (y * other.x)
            );
        }

        public override string ToString()
        {
            return $"{x}, {y}, {z}";
        }
    }
}