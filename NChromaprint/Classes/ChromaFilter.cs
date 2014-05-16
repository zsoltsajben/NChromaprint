using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace NChromaprint.Classes
{
    public class ChromaFilter : FeatureVectorConsumer
    {
        Vector<double> Coefficients { get; set; }
        int Length { get { return Coefficients.Count; } }
        Matrix<double> Buffer { get; set; }
        Vector<double> Result { get; set; }
        int BufferOffset { get; set; }
        int BufferSize { get; set; }
        FeatureVectorConsumer Consumer { get; set; }

        int BufferColumnNeeded { get; set; }


        public ChromaFilter(Vector<double> coefficients, FeatureVectorConsumer consumer)
        {
            Coefficients = coefficients;
            Buffer = null;
            Result = new DenseVector(12);
            BufferOffset = 0;
            BufferSize = 1;
            Consumer = consumer;
        }


        public void Reset()
        {
            // a mátrix 8 sora közül melyikbe kell rakni következőre
            BufferOffset = 0;

            // minimum hány sor van inicializálva eddig a mátrixban - csak akkor jut tovább az adat, ha már van annyi,
            //   mint amennyi coefficient meg volt adva inicializáláskor (Coefficients.Count)
            BufferSize = 1;
        }

        public override void Consume(Vector<double> features)
        {
            if (Buffer == null)
            {
                Buffer = new DenseMatrix(8, features.Count);
                Buffer.SetRow(BufferOffset, features);
            }
            else
            {
                Buffer.SetRow(BufferOffset, features);
            }

            BufferOffset++;
            BufferOffset %= 8;

            if (BufferSize < Length)
            {
                BufferSize++;
            }
            else
            {
                int offset = (BufferOffset + 8 - Length) % 8;

                Result.Fill(0.0);

                for (int i = 0; i < 12; i++)
                {
                    for (int j = 0; j < Length; j++)
                    {
                        Result[i] += Buffer[(offset + j) % 8, i] * Coefficients[j];
                    }
                }

                Consumer.Consume(Result);
            }
        }
    }
}
