using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace NChromaprint.Classes
{
    public class ImageBuilder : FeatureVectorConsumer
    {
        public Image Image { get; set; }


        public ImageBuilder() : this(null) { }

        public ImageBuilder(Image image)
        {
            Image = image;
        }


        public void Reset(Image image)
        {
            Image = image;
        }

        public override void Consume(Vector<double> features)
        {
            if (features.Count != Image.NeededNumCols)
                throw new ArgumentException("Length of the provided list does not equal the counts of the Image!");

            Image.AddRow(features);
        }
    }
}
