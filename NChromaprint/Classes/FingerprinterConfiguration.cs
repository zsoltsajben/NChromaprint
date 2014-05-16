using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace NChromaprint.Classes
{
    public class FingerprinterConfiguration
    {
        public Vector<double> FilterCoefficients { get; set; }
        public int NumFilterCoefficients
        {
            get { return FilterCoefficients.Count; }
        }

        public List<Classifier> Classifiers { get; set; }
        public int NumClassifiers
        {
            get { return Classifiers.Count; }
        }

        public bool Interpolate { get; set; }
        public bool RemoveSilence { get; set; }
        public int SilenceThreshold { get; set; }


        public FingerprinterConfiguration() : this(ChromaprintAlgorithm.CHROMAPRINT_ALGORITHM_TEST2) { }

        public FingerprinterConfiguration(ChromaprintAlgorithm algorithm)
        {
            Classifiers = new List<Classifier>();
            RemoveSilence = false;
            SilenceThreshold = 0;

            switch (algorithm)
            {
                case ChromaprintAlgorithm.CHROMAPRINT_ALGORITHM_TEST1:
                    SetConfiguration1();
                    break;
                case ChromaprintAlgorithm.CHROMAPRINT_ALGORITHM_TEST2:
                    SetConfiguration2();
                    break;
                case ChromaprintAlgorithm.CHROMAPRINT_ALGORITHM_TEST3:
                    SetConfiguration3();
                    break;
                case ChromaprintAlgorithm.CHROMAPRINT_ALGORITHM_TEST4:
                    SetConfiguration4();
                    break;
                default:
                    break;
            }
        }




        static readonly Vector<double> kChromaFilterCoefficients = new DenseVector(new double[] { 0.25, 0.75, 1.0, 0.75, 0.25 });

        // Used for http://oxygene.sk/lukas/2010/07/introducing-chromaprint/
        // Trained on a randomly selected test data
        private void SetConfiguration1()
        {
            Classifiers.Add(new Classifier(new Filter(0, 0, 3, 15), new Quantizer(2.10543, 2.45354, 2.69414)));
            Classifiers.Add(new Classifier(new Filter(1, 0, 4, 14), new Quantizer(-0.345922, 0.0463746, 0.446251)));
            Classifiers.Add(new Classifier(new Filter(1, 4, 4, 11), new Quantizer(-0.392132, 0.0291077, 0.443391)));
            Classifiers.Add(new Classifier(new Filter(3, 0, 4, 14), new Quantizer(-0.192851, 0.00583535, 0.204053)));
            Classifiers.Add(new Classifier(new Filter(2, 8, 2, 4), new Quantizer(-0.0771619, -0.00991999, 0.0575406)));
            Classifiers.Add(new Classifier(new Filter(5, 6, 2, 15), new Quantizer(-0.710437, -0.518954, -0.330402)));
            Classifiers.Add(new Classifier(new Filter(1, 9, 2, 16), new Quantizer(-0.353724, -0.0189719, 0.289768)));
            Classifiers.Add(new Classifier(new Filter(3, 4, 2, 10), new Quantizer(-0.128418, -0.0285697, 0.0591791)));
            Classifiers.Add(new Classifier(new Filter(3, 9, 2, 16), new Quantizer(-0.139052, -0.0228468, 0.0879723)));
            Classifiers.Add(new Classifier(new Filter(2, 1, 3, 6), new Quantizer(-0.133562, 0.00669205, 0.155012)));
            Classifiers.Add(new Classifier(new Filter(3, 3, 6, 2), new Quantizer(-0.0267, 0.00804829, 0.0459773)));
            Classifiers.Add(new Classifier(new Filter(2, 8, 1, 10), new Quantizer(-0.0972417, 0.0152227, 0.129003)));
            Classifiers.Add(new Classifier(new Filter(3, 4, 4, 14), new Quantizer(-0.141434, 0.00374515, 0.149935)));
            Classifiers.Add(new Classifier(new Filter(5, 4, 2, 15), new Quantizer(-0.64035, -0.466999, -0.285493)));
            Classifiers.Add(new Classifier(new Filter(5, 9, 2, 3), new Quantizer(-0.322792, -0.254258, -0.174278)));
            Classifiers.Add(new Classifier(new Filter(2, 1, 8, 4), new Quantizer(-0.0741375, -0.00590933, 0.0600357)));

            FilterCoefficients = kChromaFilterCoefficients;
            Interpolate = false;
        }

        // Trained on 60k pairs based on eMusic samples (mp3)
        private void SetConfiguration2()
        {
            Classifiers.Add(new Classifier(new Filter(0, 4, 3, 15), new Quantizer(1.98215, 2.35817, 2.63523)));
            Classifiers.Add(new Classifier(new Filter(4, 4, 6, 15), new Quantizer(-1.03809, -0.651211, -0.282167)));
            Classifiers.Add(new Classifier(new Filter(1, 0, 4, 16), new Quantizer(-0.298702, 0.119262, 0.558497)));
            Classifiers.Add(new Classifier(new Filter(3, 8, 2, 12), new Quantizer(-0.105439, 0.0153946, 0.135898)));
            Classifiers.Add(new Classifier(new Filter(3, 4, 4, 8), new Quantizer(-0.142891, 0.0258736, 0.200632)));
            Classifiers.Add(new Classifier(new Filter(4, 0, 3, 5), new Quantizer(-0.826319, -0.590612, -0.368214)));
            Classifiers.Add(new Classifier(new Filter(1, 2, 2, 9), new Quantizer(-0.557409, -0.233035, 0.0534525)));
            Classifiers.Add(new Classifier(new Filter(2, 7, 3, 4), new Quantizer(-0.0646826, 0.00620476, 0.0784847)));
            Classifiers.Add(new Classifier(new Filter(2, 6, 2, 16), new Quantizer(-0.192387, -0.029699, 0.215855)));
            Classifiers.Add(new Classifier(new Filter(2, 1, 3, 2), new Quantizer(-0.0397818, -0.00568076, 0.0292026)));
            Classifiers.Add(new Classifier(new Filter(5, 10, 1, 15), new Quantizer(-0.53823, -0.369934, -0.190235)));
            Classifiers.Add(new Classifier(new Filter(3, 6, 2, 10), new Quantizer(-0.124877, 0.0296483, 0.139239)));
            Classifiers.Add(new Classifier(new Filter(2, 1, 1, 14), new Quantizer(-0.101475, 0.0225617, 0.231971)));
            Classifiers.Add(new Classifier(new Filter(3, 5, 6, 4), new Quantizer(-0.0799915, -0.00729616, 0.063262)));
            Classifiers.Add(new Classifier(new Filter(1, 9, 2, 12), new Quantizer(-0.272556, 0.019424, 0.302559)));
            Classifiers.Add(new Classifier(new Filter(3, 4, 2, 14), new Quantizer(-0.164292, -0.0321188, 0.0846339)));

            FilterCoefficients = kChromaFilterCoefficients;
            Interpolate = false;
        }

        // Trained on 60k pairs based on eMusic samples with interpolation enabled (mp3)
        private void SetConfiguration3()
        {
            Classifiers.Add(new Classifier(new Filter(0, 4, 3, 15), new Quantizer(1.98215, 2.35817, 2.63523)));
            Classifiers.Add(new Classifier(new Filter(4, 4, 6, 15), new Quantizer(-1.03809, -0.651211, -0.282167)));
            Classifiers.Add(new Classifier(new Filter(1, 0, 4, 16), new Quantizer(-0.298702, 0.119262, 0.558497)));
            Classifiers.Add(new Classifier(new Filter(3, 8, 2, 12), new Quantizer(-0.105439, 0.0153946, 0.135898)));
            Classifiers.Add(new Classifier(new Filter(3, 4, 4, 8), new Quantizer(-0.142891, 0.0258736, 0.200632)));
            Classifiers.Add(new Classifier(new Filter(4, 0, 3, 5), new Quantizer(-0.826319, -0.590612, -0.368214)));
            Classifiers.Add(new Classifier(new Filter(1, 2, 2, 9), new Quantizer(-0.557409, -0.233035, 0.0534525)));
            Classifiers.Add(new Classifier(new Filter(2, 7, 3, 4), new Quantizer(-0.0646826, 0.00620476, 0.0784847)));
            Classifiers.Add(new Classifier(new Filter(2, 6, 2, 16), new Quantizer(-0.192387, -0.029699, 0.215855)));
            Classifiers.Add(new Classifier(new Filter(2, 1, 3, 2), new Quantizer(-0.0397818, -0.00568076, 0.0292026)));
            Classifiers.Add(new Classifier(new Filter(5, 10, 1, 15), new Quantizer(-0.53823, -0.369934, -0.190235)));
            Classifiers.Add(new Classifier(new Filter(3, 6, 2, 10), new Quantizer(-0.124877, 0.0296483, 0.139239)));
            Classifiers.Add(new Classifier(new Filter(2, 1, 1, 14), new Quantizer(-0.101475, 0.0225617, 0.231971)));
            Classifiers.Add(new Classifier(new Filter(3, 5, 6, 4), new Quantizer(-0.0799915, -0.00729616, 0.063262)));
            Classifiers.Add(new Classifier(new Filter(1, 9, 2, 12), new Quantizer(-0.272556, 0.019424, 0.302559)));
            Classifiers.Add(new Classifier(new Filter(3, 4, 2, 14), new Quantizer(-0.164292, -0.0321188, 0.0846339)));

            FilterCoefficients = kChromaFilterCoefficients;
            Interpolate = true;
        }

        // Same as v2, but trims leading silence
        private void SetConfiguration4()
        {
            SetConfiguration2();

            RemoveSilence = true;
            SilenceThreshold = 50;
        }
    }
}
