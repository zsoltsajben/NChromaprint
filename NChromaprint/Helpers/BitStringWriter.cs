using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint
{
    public class BitStringWriter
    {
        public List<sbyte> Value { get; private set; }

        uint Buffer { get; set; }
        int BufferSize { get; set; }


        public BitStringWriter()
        {
            Value = new List<sbyte>();
            Buffer = 0;
            BufferSize = 0;
        }


        public void Write(uint x, int bits)
        {
            Buffer |= (x << BufferSize);
            BufferSize += bits;
            while (BufferSize >= 8)
            {
                Value.Add((sbyte)(Buffer & 255));
                Buffer >>= 8;
                BufferSize -= 8;
            }
        }

        public void Flush()
        {
            while (BufferSize > 0)
            {
                Value.Add((sbyte)(Buffer & 255));
                Buffer >>= 8;
                BufferSize -= 8;
            }
            BufferSize = 0;
        }
    }
}
