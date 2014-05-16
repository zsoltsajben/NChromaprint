using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public abstract class FFTFrameConsumer
    {
        public abstract void Consume(FFTFrame frame);
    }
}
