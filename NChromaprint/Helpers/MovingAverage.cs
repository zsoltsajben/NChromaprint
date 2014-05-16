using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public class MovingAverage
    {
        List<short> Buffer { get; set; }
        int Size { get { return Buffer.Count; } }
        int Offset { get; set; }
        int Sum { get; set; }
        int Count { get; set; }


        public MovingAverage(int size)
        {
            Offset = 0;
            Sum = 0;
            Count = 0;

            Buffer = Helper.CreateShortListWithZeros(size);
            Buffer.Fill(0);
        }


        public void AddValue(short x)
        {
            Sum += x;
            Sum -= Buffer[Offset];

            if (Count < Size)
            {
                Count++;
            }

            Buffer[Offset] = x;
            Offset = (Offset + 1) % Size;
        }

        public short GetAverage()
        {
            if (Count == 0)
            {
                return 0;
            }
            else
            {
                return (short)(Sum / Count);
            }
        }
    }
}
