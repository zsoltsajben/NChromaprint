using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint
{
    public class CombinedBuffer
    {
        public int Size { get { return BufferSize[0] + BufferSize[1] - Offset; } }
        public int Offset { get; private set; }

        List<List<short>> Buffer { get; set; }
        List<int> BufferSize { get; set; }


        public CombinedBuffer(List<short> buffer1, List<short> buffer2)
        {
            Offset = 0;

            Buffer = new List<List<short>>() { buffer1, buffer2 };

            BufferSize = new List<int>() { buffer1.Count, buffer2.Count };
        }


        public int Shift(int shift)
        {
            Offset += shift;
            return Offset;
        }

        public short this[int idx]
        {
            get
            {
                idx += Offset;

                if (idx < BufferSize[0])
                {
                    return Buffer[0][idx];
                }
                else
                {
                    idx -= BufferSize[0];
                    return Buffer[1][idx];
                }
            }
            set
            {
                idx += Offset;

                if (idx < BufferSize[0])
                {
                    Buffer[0][idx] = value;
                }
                else
                {
                    idx -= BufferSize[0];
                    Buffer[1][idx] = value;
                }
            }
        }
    }
}
