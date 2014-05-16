using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace NChromaprint
{
    public abstract class FeatureVectorConsumer
    {
        public abstract void Consume(Vector<double> features);
    }
}
