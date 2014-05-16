using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public class FingerprintCalculator
    {
        List<Classifier> Classifiers { get; set; }
        int NumClassifiers { get { return Classifiers.Count; } }
        int MaxFilterWidth { get; set; }

        static readonly short[] GRAY_CODES = new short[] { 0, 1, 3, 2 };


        public FingerprintCalculator(List<Classifier> classifiers)
        {
            Classifiers = classifiers;
            MaxFilterWidth = 0;

            for (int i = 0; i < NumClassifiers; i++)
            {
                MaxFilterWidth = Math.Max(MaxFilterWidth, classifiers[i].Filter.Width);
            }

            if (MaxFilterWidth <= 0)
            {
                throw new Exception();
            }
        }


        public List<int> Calculate(Image image)
        {
            int length = image.NumRows - MaxFilterWidth + 1;

            if (length <= 0)
            {
                System.Diagnostics.Debug.WriteLine("nChromaprint.FingerprintCalculator.Calculate() -- Not " +
                    "enough data. Image has " + image.NumRows + " rows, needs at least " + MaxFilterWidth + " rows.");
                return new List<int>();
            }

            IntegralImage integralImage = new IntegralImage(image);
            List<int> fingerprint = new List<int>();

            for (int i = 0; i < length; i++)
            {
                fingerprint.Add(CalculateSubfingerprint(integralImage, i));
            }

            return fingerprint;
        }

        int CalculateSubfingerprint(IntegralImage image, int offset)
        {
            uint bits = 0;

            for (int i = 0; i < NumClassifiers; i++)
            {
                bits = (bits << 2) | (uint)GrayCode(Classifiers[i].Classify(image, offset));
            }

            return (int)bits;
        }

        int GrayCode(int i)
        {
            return GRAY_CODES[i];
        }
    }
}
