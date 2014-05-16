using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public class FingerprintDecompressor
    {
        static readonly int kMaxNormalValue = 7;
        static readonly int kNormalBits = 3;
        static readonly int kExceptionBits = 5;

        List<int> Result { get; set; }
        List<sbyte> Bits { get; set; }


        public FingerprintDecompressor()
        {
            Bits = new List<sbyte>();
        }


        public static List<int> DecompressFingerprint(List<sbyte> fingerPrint)
        {
            int? alg = null;
            return DecompressFingerprint(fingerPrint, ref alg);
        }
        public static List<int> DecompressFingerprint(List<sbyte> fingerPrint, ref int? algorithm)
        {
            var decompressor = new FingerprintDecompressor();
            return decompressor.Decompress(fingerPrint, ref algorithm);
        }

        List<int> Decompress(List<sbyte> data, ref int? algorithm)
        {
            if (algorithm != null)
            {
                algorithm = data[0];
            }

            int length =
                ((sbyte)(data[1]) << 16) |
                ((sbyte)(data[2]) << 8) |
                ((sbyte)(data[3]));

            var reader = new BitStringReader(data);
            reader.Read(8);
            reader.Read(8);
            reader.Read(8);
            reader.Read(8);

            Result = Enumerable.Repeat(-1, length).ToList();
            reader.Reset();
            ReadNormalBits(reader);
            reader.Reset();
            ReadExceptionBits(reader);
            UnpackBits();
            return Result;
        }

        void ReadNormalBits(BitStringReader reader)
        {
            int i = 0;
            while (i < Result.Count)
            {
                int bit = (int)reader.Read(kNormalBits);
                if (bit == 0)
                {
                    i++;
                }
                Bits.Add((sbyte)bit);
            }
        }

        void ReadExceptionBits(BitStringReader reader)
        {
            for (int i = 0; i < Bits.Count; i++)
            {
                if (Bits[i] == kMaxNormalValue)
                {
                    Bits[i] += (sbyte)reader.Read(kExceptionBits);
                }
            }
        }

        void UnpackBits()
        {
            int i = 0, last_bit = 0, value = 0;
            for (int j = 0; j < Bits.Count; j++)
            {
                int bit = Bits[j];
                if (bit == 0)
                {
                    Result[i] = (i > 0) ? value ^ Result[i - 1] : value;
                    value = 0;
                    last_bit = 0;
                    i++;
                    continue;
                }
                bit += last_bit;
                last_bit = bit;
                value |= 1 << (bit - 1);
            }
        }
    }
}
