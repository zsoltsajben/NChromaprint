using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public class FFT : AudioConsumer
    {
        public int FrameSize { get; private set; }
        public int Overlap { get { return FrameSize - Increment; } }

        List<double> Window { get; set; }
        int BufferOffset { get { return Buffer.Count; } }
        List<short> Buffer { get; set; }
        FFTFrame Frame { get; set; }
        int Increment { get; set; }
        FFTLib Lib { get; set; }
        FFTFrameConsumer Consumer { get; set; }


        public FFT(int frame_size, int overlap, FFTFrameConsumer consumer)
        {
            Window = Helper.CreateDoubleListWithZeros(frame_size);
            Buffer = new List<short>(frame_size);
            Frame = new FFTFrame(frame_size);
            FrameSize = frame_size;
            Increment = frame_size - overlap;
            Consumer = consumer;

            PrepareHammingWindow(Window);

            for (int i = 0; i < Window.Count; i++)
            {
                Window[i] /= short.MaxValue;
            }

            Lib = new FFTLib(frame_size, Window);
        }


        public void Reset()
        {
            Buffer.Clear();
        }

        public void Consume(List<short> input)
        {
            // Special case, just pre-filling the buffer
            if (BufferOffset + input.Count < FrameSize)
            {
                Buffer.AddRange(input);
                return;
            }

            var combinedBuffer = new CombinedBuffer(Buffer, input);
            // a 2 buffer egymás után fűzve, némi extrával

            // Apply FFT on the available data
            while (combinedBuffer.Size >= FrameSize)
            {
                Lib.ComputeFrame(combinedBuffer, Frame.Data);

                Consumer.Consume(Frame);
                combinedBuffer.Shift(Increment);
            }

            // Copy the remaining input data to the internal buffer
            Buffer = new List<short>();
            for (int i = 0; i < combinedBuffer.Size; i++)
            {
                Buffer.Add(combinedBuffer[i]);
            }
        }


        private void PrepareHammingWindow(List<double> window)
        {
            // TODO kell a -1 ?
            int max_i = window.Count - 1;
            double scale = 2.0 * Math.PI / max_i;

            for (int i = 0; i < window.Count; i++)
            {
                window[i] = 0.54 - 0.46 * Math.Cos(scale * i);
            }
        }
    }
}
