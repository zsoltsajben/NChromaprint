using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace NChromaprint
{
    public class Helper
    {
        public static List<sbyte> CreateSbyteListWithZeros(int count)
        {
            return new List<sbyte>(Enumerable.Repeat((sbyte)0, count));
        }

        public static List<short> CreateShortListWithZeros(int count)
        {
            return new List<short>(Enumerable.Repeat((short)0, count));
        }

        public static List<int> CreateIntListWithZeros(int count)
        {
            return new List<int>(Enumerable.Repeat(0, count));
        }

        public static List<float> CreateFloatListWithZeros(int count)
        {
            return new List<float>(Enumerable.Repeat((float)0, count));
        }

        public static List<double> CreateDoubleListWithZeros(int count)
        {
            return new List<double>(Enumerable.Repeat((double)0, count));
        }

        public static DenseVector CreateDoubleDenseVectorWithZeros(int count)
        {
            return new DenseVector(Enumerable.Repeat((double)0, count).ToArray());
        }
    }
}
