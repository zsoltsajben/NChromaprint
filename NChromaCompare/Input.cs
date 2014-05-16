using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NChromaCompare
{
    public class Input
    {
        public string FilePath { get; private set; }
        public List<int> Fingerprint { get; set; }

        public Input(string filePath)
        {
            FilePath = filePath;
        }
    }
}
