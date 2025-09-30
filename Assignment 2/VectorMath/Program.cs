using System;
using WindowEngine;
using VectorLibrary;

namespace WindowEngine
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===Vector Operations===");

            Vector v1 = new Vector(1, 2, 3);
            Vector v2 = new Vector(4, 5, 6);

            Console.WriteLine($"v1 = {v1}");
            Console.WriteLine($"v2 = {v2}");
            Console.WriteLine($"v1 + v2 = {v1 + v2}");
            Console.WriteLine($"v1 - v2 = {v1 - v2}");
            Console.WriteLine($"Dot product = {v1.Dot(v2)}");
            Console.WriteLine($"Cross product = {v1.Cross(v2)}");

            Console.WriteLine("\n===Matrix Operations===");

            Matrix scaleMatrix = Matrix.CreateScale(2, 2, 2);
            Console.WriteLine("Scaling Matrix:");
            Console.WriteLine(scaleMatrix);

            Matrix rotationMatrix = Matrix.CreateRotationZ((float)Math.PI / 4); // 45 degrees
            Console.WriteLine("Rotation Matrix (Z-axis):");
            Console.WriteLine(rotationMatrix);

            Matrix combinedMatrix = scaleMatrix * rotationMatrix;
            Console.WriteLine("Combined (Scale * Rotation):");
            Console.WriteLine(combinedMatrix);

            Vector vector = new Vector(1, 0, 0);
            Vector transformed = combinedMatrix.Transform(vector);
            Console.WriteLine($"Original Vector: {vector}");
            Console.WriteLine($"Transformed Vector: {transformed}");
        }
    }
}
