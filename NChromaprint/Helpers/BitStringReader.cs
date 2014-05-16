using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint
{
    public class BitStringReader
    {
        List<sbyte> Value { get; set; }
        int ValueIdx { get; set; }
        uint Buffer { get; set; }
        int BufferSize { get; set; }


        public BitStringReader(List<sbyte> input)
        {
            Value = input;
            Buffer = 0;
            BufferSize = 0;
            ValueIdx = 0;
        }


        public uint Read(int bits)
        {
            if (BufferSize < bits) {
                for (; ValueIdx < Value.Count; ValueIdx++)
                {
                    Buffer |= (uint)Value[ValueIdx] << BufferSize;
                    BufferSize += 8;
                }
			}
			uint result = (uint)(Buffer & ((1 << bits) - 1));
			Buffer >>= bits;
			BufferSize -= bits;
			return result;
        }

        public void Reset()
        {
            Buffer = 0;
            BufferSize = 0;
        }
    }
}
