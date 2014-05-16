using NChromaprint.Enums;
using System;
using System.Collections.Generic;

namespace NChromaprint.Classes
{
    public class Fingerprinter : AudioConsumer
    {
        static readonly int SAMPLE_RATE = 11025;
        static readonly int FRAME_SIZE = 4096;
        static readonly int OVERLAP = FRAME_SIZE - FRAME_SIZE / 3;
        static readonly int MIN_FREQ = 28;
        static readonly int MAX_FREQ = 3520;

        Image image;
        ImageBuilder imageBuilder;
        ChromaNormalizer chromaNormalizer;
        ChromaFilter chromaFilter;
        Chroma chroma;
        FFT fft;
        SilenceRemover silenceRemover;
        AudioProcessor audioProcessor;

        FingerprintCalculator fingerprintCalculator;
        FingerprinterConfiguration fingerprinterConfiguration;


        public Fingerprinter(FingerprinterConfiguration fpConfig)
        {
            if (fpConfig == null)
            {
                fpConfig = new FingerprinterConfiguration();
            }

            image = new Image(12);
            imageBuilder = new ImageBuilder(image);
            chromaNormalizer = new ChromaNormalizer(imageBuilder);
            chromaFilter = new ChromaFilter(fpConfig.FilterCoefficients, chromaNormalizer);
            chroma = new Chroma(MIN_FREQ, MAX_FREQ, FRAME_SIZE, SAMPLE_RATE, chromaFilter);
            fft = new FFT(FRAME_SIZE, OVERLAP, chroma);

            if (fpConfig.RemoveSilence)
            {
                silenceRemover = new SilenceRemover(fft);
                silenceRemover.Threshold = fpConfig.SilenceThreshold;
                audioProcessor = new AudioProcessor(SAMPLE_RATE, silenceRemover);
            }
            else
            {
                silenceRemover = null;
                audioProcessor = new AudioProcessor(SAMPLE_RATE, fft);
            }

            fingerprintCalculator = new FingerprintCalculator(fpConfig.Classifiers);
            fingerprinterConfiguration = fpConfig;
        }


        public bool SetOption(FingerprinterOption option, int value)
        {
            switch (option)
            {
                case FingerprinterOption.SilenceThreshold:
                    if (silenceRemover != null)
                    {
                        silenceRemover.Threshold = value;
                        return true;
                    }
                    break;

                default:
                    break;
            }

            return false;
        }

        public bool Start(int sample_rate, int num_channels)
        {
            if (!audioProcessor.Reset(sample_rate, num_channels))
            {
                return false;
            }

            fft.Reset();
            chroma.Reset();
            chromaFilter.Reset();
            chromaNormalizer.Reset();
            image = new Image(12);
            imageBuilder.Reset(image);

            return true;
        }

        public void Consume(List<short> samples)
        {
            audioProcessor.Consume(samples);
        }

        public List<int> Finish()
        {
            audioProcessor.Flush();
            return fingerprintCalculator.Calculate(image);
        }
    }
}
