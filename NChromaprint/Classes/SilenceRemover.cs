using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.Classes
{
    public class SilenceRemover : AudioConsumer
    {
        public int Threshold { get; set; }
        public AudioConsumer Consumer { get; set; }

        bool IsStart { get; set; }
        MovingAverage Average { get; set; }

        const short _silenceWindowLength = 55; // 5 ms as 11025 Hz


        public SilenceRemover(AudioConsumer consumer, int threshold = 0)
        {
            IsStart = true;
            Threshold = threshold;
            Average = new MovingAverage(_silenceWindowLength);
            Consumer = consumer;
        }


        public bool Reset(int sample_rate, int num_channels)
        {
            if (num_channels != 1)
            {
                System.Diagnostics.Debug.WriteLine("nChromaprint.SilenceRemover.Reset() -- Expecting mono audio signal.");
                return false;
            }
            else
            {
                IsStart = true;
                return true;
            }
        }

        public void Consume(List<short> input)
        {
            if (IsStart)
            {
                while (input.Count > 0)
                {
                    Average.AddValue(Math.Abs(input[0]));
                    if (Average.GetAverage() > Threshold)
                    {
                        IsStart = false;
                        break;
                    }
                    input.RemoveAt(0);
                }
            }

            if (input.Count != 0)
            {
                Consumer.Consume(input);
            }
        }

        public void Flush()
        { }
    }
}
