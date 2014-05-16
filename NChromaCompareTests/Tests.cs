using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NChromaCompare;
using System.IO;

namespace NCromaprintTests
{
    [TestClass]
    public class Tests
    {
        static readonly string audiofolder = @"\..\..\Audio\";

        static readonly string wav = "Plurabelle - Lips.wav";
        static readonly string mp3_80kbps = "Plurabelle - Lips 80kbps.mp3";
        static readonly string mp3_128kbps = "Plurabelle - Lips 128kbps.mp3";
        static readonly string mp3_128kbps_delay = "Plurabelle - Lips 128kbps with delay.mp3";


        [TestMethod]
        public void Test_Format()
        {
            AreSimilar(mp3_128kbps, wav);
        }

        [TestMethod]
        public void Test_BitRate()
        {
            AreSimilar(mp3_128kbps, mp3_80kbps);
        }

        [TestMethod]
        public void Test_Delay()
        {
            AreSimilar(mp3_128kbps, mp3_128kbps_delay);
        }

        private void AreSimilar(string path1, string path2)
        {
            var dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            path1 = dir + audiofolder + path1;
            path2 = dir + audiofolder + path2;

            var inputs = new List<Input>()
            {
                new Input(path1),
                new Input(path2)
            };

            var comparer = new NChromaComparer();
            var result = comparer.CompareFiles(inputs);

            Assert.IsTrue(result[path1][path2] >= 0.9);
        }
    }
}
