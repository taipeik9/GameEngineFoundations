using System;

namespace VectorLibrary
{
    public class Matrix
    {
        public float[,] Data = new float[4, 4];

        public Matrix() { }

        // identity matrix
        public static Matrix Identity()
        {
            Matrix identityMatrix = new();
            for (int i = 0; i < 4; ++i)
            {
                identityMatrix.Data[i, i] = 1.0f;
            }
            return identityMatrix;
        }

        // scaling matrix
        public static Matrix CreateScale(float x, float y, float z)
        {
            Matrix identityMatrix = new();
            identityMatrix.Data[0, 0] = x;
            identityMatrix.Data[1, 1] = y;
            identityMatrix.Data[2, 2] = z;
            return identityMatrix;
        }

        // rotation matrix - around the z axis
        public static Matrix CreateRotationZ(float radians)
        {
            Matrix identityMatrix = new();

            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            identityMatrix.Data[0, 0] = cos;
            identityMatrix.Data[0, 1] = -sin;
            identityMatrix.Data[1, 0] = sin;
            identityMatrix.Data[1, 1] = cos;
            return identityMatrix;
        }

        public static Matrix operator *(Matrix a, Matrix b)
        {
            Matrix result = new();

            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    float sum = 0;
                    for (int k = 0; k < 4; k++)
                        sum += a.Data[row, k] * b.Data[k, col];
                    result.Data[row, col] = sum;
                }
            }
            return result;
        }

        // apply transformation to a vector
        public Vector Transform(Vector v)
        {
            float x = v.x * Data[0, 0] + v.y * Data[0, 1] + v.z * Data[0, 2] + Data[0, 3];
            float y = v.x * Data[1, 0] + v.y * Data[1, 1] + v.z * Data[1, 2] + Data[1, 3];
            float z = v.x * Data[2, 0] + v.y * Data[2, 1] + v.z * Data[2, 2] + Data[2, 3];
            return new Vector(x, y, z);
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    s += $"{Data[i, j],6:F2} ";
                }
                s += "\n";
            }
            return s;
        }
    }
}