using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace NChromaprint.Classes
{
    public class ChromaNormalizer : FeatureVectorConsumer
    {
        private FeatureVectorConsumer Consumer { get; set; }


        public ChromaNormalizer(FeatureVectorConsumer consumer)
        {
            Consumer = consumer;
        }


        public void Reset()
        { }

        public override void Consume(Vector<double> features)
        {
            NormalizeVector(features, EuclideanNorm, 0.01);

            Consumer.Consume(features);
        }


        private void NormalizeVector(Vector<double> list, Func<Vector<double>, double> func, double threshold = 0.01)
        {
            double norm = func(list);

            if (norm < threshold)
                list.Fill(0.0);

            else
                for (int i = 0; i < list.Count; i++)
                    list[i] /= norm;
        }

        private double EuclideanNorm(Vector<double> list)
        {
            double sum_squares = 0.0;

            for (int i = 0; i < list.Count; i++)
                sum_squares += list[i] * list[i];

            if (sum_squares > 0)
            {
                return Math.Sqrt(sum_squares);
            }
            else
            {
                return 0.0;
            }
        }
    }
}
