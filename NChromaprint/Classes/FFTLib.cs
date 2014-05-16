using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.IntegralTransforms.Algorithms;

namespace NChromaprint.Classes
{
    public class FFTLib
    {
        List<double> Window { get; set; }
        int FrameSize { get; set; }
        List<float> Input { get; set; }
        DiscreteFourierTransform Dft { get; set; }


        public FFTLib(int frame_size, List<double> window)
        {
            FrameSize = frame_size;
            Window = window;

            Input = Helper.CreateFloatListWithZeros(frame_size);

            // DFT inicializálása
            Dft = new DiscreteFourierTransform();
        }

        /*public ~FFTLib()
        {
            av_rdft_end()......
        }*/

        public void ComputeFrame(CombinedBuffer input, List<double> output)
        {
            // Hamming window alkalmazása
            ApplyWindow(input, Window, Input, FrameSize);

            // input: frame_size darab float

            // komplex számok a valósakból
            var samples = Input.Select(item => new Complex(item, 0.0)).ToArray();
            // FFT elvégzése
            //Dft.BluesteinForward(samples, FourierOptions.NoScaling);
            Dft.Radix2Forward(samples, FourierOptions.NoScaling);

            // az eredménynek csak a fele kell, mert a bemenet valós volt, de komplex DFT-t használunk
            // lásd: http://www.dspguide.com/ch12/1.htm
            for (int i = 0; i < samples.Length / 2; i++)
            {
                output[i] = samples[i].GetSquaredMagnitude();
            }
        }

        private void ApplyWindow(CombinedBuffer input, List<double> window, List<float> output, int size)
        {
            for (int i = 0; i < size; i++)
            {
                output[i] = (float)(input[i] * window[i]);
            }
        }
    }
}
