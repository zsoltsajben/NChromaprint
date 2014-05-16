using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public class FFTFrame
    {
        public int Size { get; private set; }
        public List<double> Data { get; set; }


        public FFTFrame(int size)
        {
            Size = size;
            Data = Helper.CreateDoubleListWithZeros(size);
        }


        public double Magnitude(int i)
        {
            return Math.Sqrt(Energy(i));
        }

        public double Energy(int i)
        {
            return Data[i];
        }
    }
}
