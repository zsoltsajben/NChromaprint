using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public class Classifier
    {
        public Filter Filter { get; set; }
        public Quantizer Quantizer { get; set; }


        public Classifier() : this(new Filter(), new Quantizer()) { }
        public Classifier(Filter f) : this(f, new Quantizer()) { }
        public Classifier(Quantizer q) : this(new Filter(), q) { }
        public Classifier(Filter filter, Quantizer quantizer)
        {
            Filter = filter;
            Quantizer = quantizer;
        }


        public int Classify(IntegralImage image, int offset)
        {
            double value = Filter.Apply(image, offset);
            return Quantizer.Quantize(value);
        }

        public override string ToString()
        {
            return "Classifier(" + Filter.ToString() + ", " + Quantizer.ToString() + ")";
        }
    }
}
