using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace NChromaprint.Classes
{
    public class AudioProcessor : AudioConsumer
    {
        static readonly int _minSampleRate = 1000;
        static readonly int _maxBufferSize = 1024 * 16;

        public int TargetSampleRate { get; set; }
        public AudioConsumer Consumer { get; set; }

        List<short> InputBuffer { get; set; }
        int BufferOffset { get { return InputBuffer.Count; } }
        int BufferSize { get { return _maxBufferSize; } }
        int NumChannels { get; set; }

        MemoryStream ResampleInput { get; set; }
        WaveFormat InputFormat { get; set; }
        WaveFormat OutputFormat { get; set; }

        MediaFoundationResampler ResampleOutput { get; set; }
        //WaveFormatConversionStream ResampleOutput { get; set; }

        public AudioProcessor(int sample_rate, AudioConsumer consumer)
        {
            TargetSampleRate = sample_rate;
            Consumer = consumer;
            ResampleInput = null;
            InputBuffer = new List<short>();
        }

        //! Prepare for a new audio stream
        public bool Reset(int sample_rate, int num_channels)
        {
            if (num_channels <= 0)
            {
                Debug.WriteLine("NChromaprint::AudioProcessor::Reset() -- No audio channels.");
                return false;
            }
            if (sample_rate <= _minSampleRate)
            {
                Debug.WriteLine("NChromaprint::AudioProcessor::Reset() -- Sample rate less than " +
                    _minSampleRate + " (" + sample_rate + ").");
                return false;
            }

            InputBuffer.Clear();

            if (ResampleInput != null)
            {
                ResampleInput.Dispose();
                ResampleInput = null;
            }
            if (sample_rate != TargetSampleRate)
            {
                InputFormat = new WaveFormat(sample_rate, 1);
                OutputFormat = new WaveFormat(TargetSampleRate, 1);

                ResampleInput = new MemoryStream();

                // init resampling
                var inputWaveStream = new RawSourceWaveStream(ResampleInput, InputFormat);
                //ResampleOutput = new WaveFormatConversionStream(OutputFormat, inputWaveStream);
                ResampleOutput = new MediaFoundationResampler(inputWaveStream, OutputFormat);
                ResampleOutput.ResamplerQuality = 9;

                ResampleInput.Position = 0;


                /*ResampleCtx = av_resample_init(
                    out_rate:           TargetSampleRate,       - 11025 - 
                    in_rate:            sample_rate,            - 44100 - 
                    filter_length:      kResampleFilterLength,  - 16    - length of each FIR filter in the filterbank relative to the cutoff freq 
                    log2_phase_count:   kResamplePhaseCount,    - 10    - log2 of the number of entries in the polyphase filterbank 
                    linear:             kResampleLinear,        - 0     - If 1 then the used FIR filter will be linearly interpolated between the 2 closest, if 0 the closest will be used 
                    cutoff:             kResampleCutoff);       - 0.8   - cutoff frequency, 1.0 corresponds to half the output sampling rate 
                 */
            }

            NumChannels = num_channels;
            return true;
        }

        //! Process a chunk of data from the audio stream
        public void Consume(List<short> input)
        {
            if (input.Count % NumChannels != 0)
            {
                throw new ArgumentException();
            }

            while (input.Count > 0)
            {
                int consumed = Load(input, input.Count / NumChannels);
                input.RemoveRange(0, consumed * NumChannels);

                if (BufferSize == BufferOffset)
                {
                    Resample();

                    if (BufferSize == BufferOffset)
                    {
                        Debug.WriteLine("nChromaprint.AudioProcessor.Consume() -- Resampling failed?");
                        return;
                    }
                }
            }
        }

        #region Load channels

        int Load(List<short> input, int length)
        {
            // HACK a max buffer méret végtelen, így leszünk a legközelebb a szükséges végeredményhez...
            /*if (BufferOffset > BufferSize)
            {
                throw new Exception();
            }

            length = Math.Min(length, BufferSize - BufferOffset);*/

            switch (NumChannels)
            {
                case 1:
                    LoadMono(input, length);
                    break;
                case 2:
                    LoadStereo(input, length);
                    break;
                default:
                    LoadMultiChannel(input, length);
                    break;
            }

            return length;
        }

        void LoadMono(List<short> input, int length)
        {
            InputBuffer.AddRange(input.Take(length));
        }

        void LoadStereo(List<short> input, int length)
        {
            for (int i = 0; i < length; i++)
            {
                InputBuffer.Add((short)(((int)input[2 * i] + (int)input[2 * i + 1]) / 2));
            }
        }

        void LoadMultiChannel(List<short> input, int length)
        {
            for (int i = 0; i < length; i++)
            {
                long sum = 0;
                for (int j = 0; j < NumChannels; j++)
                {
                    sum += input[i * NumChannels + j];
                }
                InputBuffer.Add((short)(sum / NumChannels));
            }
        }

        #endregion // end of Load channels

        void Resample()
        {
            if (ResampleInput == null)
            {
                Consumer.Consume(InputBuffer);
                InputBuffer.Clear();
                return;
            }

            // HACK az NAudio nem tud ilyen paraméterezést...
            /*int length = av_resample(         - 4086  - Returns: the number of samples written in dst or -1 if an error occurred
                c:          m_resample_ctx,     - 
                dst:        m_resample_buffer,  - 
                src:        m_buffer,           -       - an array of unconsumed samples 
                consumed:   & consumed,         -0-16305- the number of samples of src which have been consumed are returned here 
                src_size:   m_buffer_offset,    - 16384 - the number of unconsumed samples available 
                dst_size:   kMaxBufferSize,     - 16384 - the amount of space in samples available in dst 
                update_ctx: 1                   - 1     - If this is 0 then the context will not be modified, that way several
                                                        channels can be resampled with the same context. 
                );*/

            // nem szabad új Streamet tenni a ResampleInput helyére,
            //   mert a kimeneti Stream nem fogja tudni olvasni onnantól

            // kiürítjük a korábbi adatokat
            ResampleInput.Position = 0;
            ResampleInput.SetLength(0);

            // beadjuk a bemeneti adatokat a mintavételezőnek
            var inputShorts = InputBuffer.ToArray();
            var inputBytes = new byte[inputShorts.Length * 2];
            Buffer.BlockCopy(inputShorts, 0, inputBytes, 0, inputBytes.Length);
            ResampleInput.Write(inputBytes, 0, inputBytes.Length);

            // visszaállítjuk a streamet az elejére
            ResampleInput.Position = 0;

            // legalább egy másodpercnyi adatot szeretnénk egyszerre kiolvasni
            int bytesPerSecond = ResampleOutput.WaveFormat.AverageBytesPerSecond;
            if (bytesPerSecond % 2 == 1)
                bytesPerSecond++;

            // kiolvassuk a mintavett értékeket
            var outputBytes = new byte[bytesPerSecond];
            var outputShorts = new List<short>();
            int readCount;
            while ((readCount = ResampleOutput.Read(outputBytes, 0, bytesPerSecond)) > 0)
            {
                var tempShorts = new short[readCount / 2];
                Buffer.BlockCopy(outputBytes, 0, tempShorts, 0, readCount);
                outputShorts.AddRange(tempShorts);
            }

            // továbbküldjük az eredményt
            Consumer.Consume(outputShorts);

            InputBuffer.Clear();
            return;
        }

        //! Process any buffered input that was not processed before and clear buffers
        public void Flush()
        {
            if (BufferOffset != 0)
            {
                Resample();
            }

            try
            {
                ResampleInput.Dispose();
            }
            catch { }
            ResampleInput = null;
        }

    }
}
