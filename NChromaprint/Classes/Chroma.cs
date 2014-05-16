using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Generic;

namespace NChromaprint.Classes
{
    public class Chroma : FFTFrameConsumer
    {
        static readonly int NUM_BANDS = 12;

        public bool Interpolate { get; set; }
        List<sbyte> Notes { get; set; }
        Vector<double> NotesFrac { get; set; }
        int MinIndex { get; set; }
        int MaxIndex { get; set; }
        Vector<double> Features { get; set; }
        FeatureVectorConsumer Consumer { get; set; }

        public Chroma(int min_freq, int max_freq, int frame_size, int sample_rate, FeatureVectorConsumer consumer)
        {
            Interpolate = false;
            Notes = Helper.CreateSbyteListWithZeros(frame_size);
            NotesFrac = Helper.CreateDoubleDenseVectorWithZeros(frame_size);
            Features = Helper.CreateDoubleDenseVectorWithZeros(NUM_BANDS);
            Consumer = consumer;

            PrepareNotes(min_freq, max_freq, frame_size, sample_rate);
        }

        public void Reset()
        { }

        public override void Consume(FFTFrame frame)
        {
            Features.Fill(0.0);

            for (int i = MinIndex; i < MaxIndex; i++)
            {
                int note = Notes[i];
                double energy = frame.Energy(i);

                // ezt szerintem nem használjuk sehol igazából...
                if (Interpolate)
                {
                    int note2 = note;
                    double a = 1.0;

                    if (NotesFrac[i] < 0.5)
                    {
                        note2 = (note + NUM_BANDS - 1) % NUM_BANDS;
                        a = 0.5 + NotesFrac[i];
                    }
                    if (NotesFrac[i] > 0.5)
                    {
                        note2 = (note + 1) % NUM_BANDS;
                        a = 1.5 - NotesFrac[i];
                    }

                    Features[note] += energy * a;
                    Features[note2] += energy * (1.0 - a);
                }
                else
                {
                    Features[note] += energy;
                }
            }
            Consumer.Consume(Features);
        }

        // min_freq = 28; max_freq = 3520; frame_size = 4096; sample_rate = 11025;
        private void PrepareNotes(int min_freq, int max_freq, int frame_size, int sample_rate)
        {
            // azokon a hangokon fog végigmenni, amik egész periódusokban beleférnek az ablakhosszba, és ezek közül is csak
            //    a 28Hz-hez legközelebbi ilyen hangtól a 3520Hz-hez legközelebbiig

            // melyik indextől kell majd a feldolgozásnál elindulnia, hogy annak a frekvenciája kb 28 Hz legyen
            MinIndex = Math.Max(1, FreqToIndex(min_freq, frame_size, sample_rate));
            // melyik indexig kell figyelembe vennie a kapott értékeket - kb 3502 Hz a felső határ
            MaxIndex = Math.Min(frame_size / 2, FreqToIndex(max_freq, frame_size, sample_rate));
            // MinIndex = 10; MaxIndex = 1308;

            for (int i = MinIndex; i < MaxIndex; i++)
            {
                // melyik frekvencia fog tartozni ahhoz a kapott indexhez
                //   (melyik frekvenciájú hang fér bele i periódussal az ablakba)
                double freq = IndexToFreq(i, frame_size, sample_rate);
                // az a frekvencia hány oktávra van az A0-tól
                double octave = FreqToOctave(freq);
                // az oktávban mért távolság törtrésze * 12
                double note = NUM_BANDS * (octave - Math.Floor(octave));    // NUM_BANDS = 12;
                // ? gondolom az egészrésze
                Notes[i] = (sbyte)note;
                NotesFrac[i] = note - Notes[i];
            }
        }

        // hány periódus fér bele egy ablakba az adott frekvenciából
        private int FreqToIndex(double freq, int frame_size, int sample_rate)
        {
            return (int)Math.Round(frame_size * freq / sample_rate);
        }

        // melyik az a frekvencia, aminek a periódusa pont i-szer fér bele az ablakba
        private double IndexToFreq(int i, int frame_size, int sample_rate)
        {
            return ((double)i) * sample_rate / frame_size;
        }

        // hány oktáv van az adott freq és a base között (base = 440/16 Hz = 27,5 Hz, az A0-hoz tartozó frekvencia)
        private double FreqToOctave(double freq, double base_ = 440.0/16.0)
        {
            return Math.Log(freq / base_) / Math.Log(2.0);
        }

    }
}
