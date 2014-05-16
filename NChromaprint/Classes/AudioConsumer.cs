using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public interface AudioConsumer
    {
        void Consume(List<short> input);
    }
}
