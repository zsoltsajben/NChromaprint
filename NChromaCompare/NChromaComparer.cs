using NChromaprint.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NChromaCompare
{
    public class NChromaComparer
    {
        public int MaxAudioLength { get; set; }

        public NChromaComparer()
        {
            MaxAudioLength = 120;
        }

        public Dictionary<string, Dictionary<string, float>> CompareFiles(IEnumerable<Input> inputs)
        {
            var inputList = inputs.ToList();

            // fingerprintek kiszámolása
            for (int i = 0; i < inputList.Count; i++)
            {
                var input = inputList[i];
                if (input.Fingerprint == null)
                {
                    List<int> fingerprint;
                    var ctx = new NChromaprintContext();

                    if (!ctx.GetRawFingerprintForFile(input.FilePath, out fingerprint, MaxAudioLength))
                    {
                        throw new Exception(
                            "Couldn't process all of the files, at least one of them is in an unsupported format!");
                    }

                    input.Fingerprint = fingerprint;
                }
            }

            // hasonlóság mértékének megállapítása
            var result = new Dictionary<string, Dictionary<string, float>>();
            for (int i = 0; i < inputList.Count; i++)
            {
                var input1 = inputList[i];

                for (int j = i + 1; j < inputList.Count; j++)
                {
                    var input2 = inputList[j];

                    float similarity = CalculateSimilarity(input1, input2);

                    AddSimilarity(result, input1.FilePath, input2.FilePath, similarity);
                    AddSimilarity(result, input2.FilePath, input1.FilePath, similarity);
                }
            }

            return result;
        }

        private float CalculateSimilarity(Input input1, Input input2)
        {
            // kiválasztjuk, hogy melyik a rövidebb és melyik a hosszabb
            List<int> shorterFp, longerFp;
            if (input1.Fingerprint.Count() < input2.Fingerprint.Count())
            {
                shorterFp = input1.Fingerprint;
                longerFp = input2.Fingerprint;
            }
            else
            {
                shorterFp = input2.Fingerprint;
                longerFp = input1.Fingerprint;
            }

            int shorterCount = shorterFp.Count();
            int longerCount = longerFp.Count();

            // a rövid csúsztatását mennyivel előbbről kezdjük (a hosszú méretének %-ában)
            var longPercentage = 5;
            // a rövidnek ennyi %-a kell, hogy átfedésben maradjon minimum, amikor kezdjük a csúsztatást
            //   (tehát amikor maximálisan "lelóg" a hosszúról)
            var shortPercentage = 70;

            // kiszámoljuk az előző hosszakat egységben, a megadott %-ok alaján
            var maxNotOverlapped = (int)Math.Round(longerCount * (float)longPercentage / 100);
            var minOverlap = (int)Math.Round(shorterCount * (float)shortPercentage / 100);

            // leellenőrizzük, hogy meglesz-e a meghatározott minimális átfedés
            if (shorterCount - maxNotOverlapped < minOverlap)
                return -1;

            // a rövidet csúsztatjuk a hosszún, és megnézzük, hogy hol a legnagyobb az egyezés
            var maxSimilarityCount = 0;
            for (int offset = -maxNotOverlapped; offset < longerCount - (shorterCount - maxNotOverlapped) + 1; offset++)
            {
                int similarityCount = 0;

                // az indexelés attól függ, hogy hol van átfedés épp
                int i, j;
                if (offset < 0)
                {
                    i = -offset;
                    j = 0;
                }
                else
                {
                    i = 0;
                    j = offset;
                }

                // összehasonlítjuk az összes bitet
                for (; i < shorterCount && j < longerCount; i++, j++)
                {
                    // XOR után azok a bitek lesznek igaz értékűek, ahol eltérés volt
                    similarityCount += NumberOfUnsetBits(shorterFp[i] ^ longerFp[j]);
                }

                // ha nagyobb egyezést találtunk, mint eddig
                if (similarityCount > maxSimilarityCount)
                    maxSimilarityCount = similarityCount;

                // ha 100%-os egyezést találtunk
                if (maxSimilarityCount == shorterCount * 32)
                    break;
            }

            return (float)maxSimilarityCount / shorterCount / 32;
        }

        private void AddSimilarity(Dictionary<string, Dictionary<string, float>> similarities,
            string input1, string input2, float similarity)
        {
            if (!similarities.ContainsKey(input1))
            {
                similarities.Add(input1, new Dictionary<string, float>());
            }
            if (similarities[input1] == null)
            {
                similarities[input1] = new Dictionary<string, float>();
            }
            similarities[input1][input2] = similarity;
        }

        private int NumberOfUnsetBits(int i)
        {
            return 32 - NumberOfSetBits(i);
        }

        private int NumberOfSetBits(int i)
        {
            return new System.Collections.BitArray(new int[] { i }).Cast<bool>().Where(b => b == true).Count();
            //i = i - ((i >> 1) & 0x55555555);
            //i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            //return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }
    }
}
