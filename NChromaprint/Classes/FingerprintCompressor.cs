using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public class FingerprintCompressor
    {
        static readonly int kMaxNormalValue = 7;
        static readonly int kNormalBits = 3;
        static readonly int kExceptionBits = 5;

        List<sbyte> Result { get; set; }
        List<sbyte> Bits { get; set; }


        public FingerprintCompressor()
        {
            Bits = new List<sbyte>();
        }


        public static List<sbyte> CompressFingerprint(List<int> fingerPrint, int algorithm = 0)
        {
            var compressor = new FingerprintCompressor();
            return compressor.Compress(fingerPrint, algorithm);
        }

        public List<sbyte> Compress(List<int> data, int algorithm = 0)
        {
            if (data.Count > 0)
            {
                ProcessSubFingerprint((uint)data[0]);
                for (int i = 1; i < data.Count; i++)
                {
                    ProcessSubFingerprint((uint)(data[i] ^ data[i - 1]));
                }
            }

            int length = data.Count;
            Result = new List<sbyte>();
            Result.Add((sbyte)(algorithm & 255));
            Result.Add((sbyte)((length >> 16) & 255));
            Result.Add((sbyte)((length >> 8) & 255));
            Result.Add((sbyte)((length) & 255));

            WriteNormalBits();
            WriteExceptionBits();

            return Result;
        }

        void ProcessSubFingerprint(uint x)
        {
            int bit = 1, last_bit = 0;
            while (x != 0)
            {
                if ((x & 1) != 0)
                {
                    Bits.Add((sbyte)(bit - last_bit));
                    last_bit = bit;
                }
                x >>= 1;
                bit++;
            }
            Bits.Add((sbyte)0);
        }

        void WriteNormalBits()
        {
            var writer = new BitStringWriter();
            for (int i = 0; i < Bits.Count; i++)
            {
                writer.Write((uint)Math.Min((int)(Bits[i]), kMaxNormalValue), kNormalBits);
            }
            writer.Flush();
            Result.AddRange(writer.Value);
        }

        void WriteExceptionBits()
        {
            var writer = new BitStringWriter();
            for (int i = 0; i < Bits.Count; i++)
            {
                if (Bits[i] >= kMaxNormalValue)
                {
                    writer.Write((uint)((int)(Bits[i]) - kMaxNormalValue), kExceptionBits);
                }
            }
            writer.Flush();
            Result.AddRange(writer.Value);
        }
    }
}
