using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace NChromaprint
{
    public static class Extensions
    {
        public static void Fill(this Vector<double> vector, double value)
        {
            for (int i = 0; i < vector.Count; i++)
            {
                vector[i] = value;
            }
        }

        public static void Fill(this List<short> vector, short value)
        {
            for (int i = 0; i < vector.Count; i++)
            {
                vector[i] = value;
            }
        }

        public static void Fill(this List<sbyte> vector, sbyte value)
        {
            for (int i = 0; i < vector.Count; i++)
            {
                vector[i] = value;
            }
        }

        public static double GetSquaredMagnitude(this Complex complex)
        {
            return complex.Real * complex.Real + complex.Imaginary * complex.Imaginary;
        }
    }
}
