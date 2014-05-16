using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace nChromaprint
{
    public class AudioProcessor : AudioConsumer
    {
        static readonly int kMinSampleRate = 1000;
        static readonly int kMaxBufferSize = 1024 * 16;

        // Resampler configuration
        static readonly int kResampleFilterLength = 16;
        static readonly int kResamplePhaseCount = 10;
        static readonly int kResampleLinear = 0;
        static readonly double kResampleCutoff = 0.8;

        public int TargetSampleRate { get; set; }
        public AudioConsumer Consumer { get; set; }

        List<short> Buffer { get; set; }
        List<short> ResampleBuffer { get; set; }
        int BufferOffset { get; set; }
        int BufferSize { get { return Buffer.Count; } }
        int NumChannels { get; set; }
        //Object ResampleCtx { get; set; }

        MemoryStream ResampleInputMemoryStream { get; set; }
        //RawSourceWaveStream ResampleInputWaveStream { get; set; }
        WaveFormat ResampleInputFormat { get; set; }
        WaveFormat ResampleOutputFormat { get; set; }
        //WaveFormatConversionStream ResampleOutputStream { get; set; }


        public AudioProcessor(int sample_rate, AudioConsumer consumer)
        {
            TargetSampleRate = sample_rate;
            Consumer = consumer;
            //ResampleCtx = null;
            ResampleInputMemoryStream = null;

            Buffer = Helper.CreateShortListWithZeros(kMaxBufferSize);
            BufferOffset = 0;
            ResampleBuffer = new List<short>();
        }

        //! Prepare for a new audio stream
        public bool Reset(int sample_rate, int num_channels)
        {
            if (num_channels <= 0)
            {
                Debug.WriteLine("Chromaprint::AudioProcessor::Reset() -- No audio channels.");
                return false;
            }
            if (sample_rate <= kMinSampleRate)
            {
                Debug.WriteLine("Chromaprint::AudioProcessor::Reset() -- Sample rate less than " +
                    kMinSampleRate + " (" + sample_rate + ").");
                return false;
            }
            
            BufferOffset = 0;

            // TODO ?
            if (ResampleInputMemoryStream != null)
            {
                //av_resample_close(ResampleCtx);

                ResampleInputMemoryStream.Dispose();
                ResampleInputMemoryStream = null;
            }
            if (sample_rate != TargetSampleRate)
            {
                ResampleInputMemoryStream = new MemoryStream();

                ResampleInputFormat = new WaveFormat(sample_rate, 1/*num_channels*/);
                ResampleOutputFormat = new WaveFormat(TargetSampleRate, 1/*num_channels*/);

                //WaveFormat inFormat = new WaveFormat(sample_rate, num_channels);
                //ResampleInputWaveStream = new RawSourceWaveStream(ResampleInputMemoryStream, inFormat);  //ResamplerDmoStream conversion = new ResamplerDmoStream(stream, new WaveFormat(TargetSampleRate, NumChannels));
                //ResampleOutputFormat = new WaveFormat(TargetSampleRate, num_channels);

                //ResampleOutputStream = new WaveFormatConversionStream(outFormat, sourceStream);

                //throw new NotImplementedException("nChromaprint.AudioProcessor.Reset() -- Resampling is not yet implemented!");

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
        public override void Consume(List<short> input, int length)
        {
            if (length < 0 || length % NumChannels != 0)
            {
                throw new ArgumentException();
            }

            //input = new List<short>(input);

            length /= NumChannels;

            while (length > 0)
            {
                int consumed = Load(input, length);
                // TODO: megmaradhatna, ha olyanra akarom, mint az eredetiben...
                input.RemoveRange(0, consumed * NumChannels);
                length -= consumed;

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

        //! Process any buffered input that was not processed before and clear buffers
        public void Flush()
        {
            if (BufferOffset != 0)
            {
                Resample();
            }

            // TODO ?
            //ResampleInputStream.Dispose();
            //ResampleInputStream = null;
        }


        int Load(List<short> input, int length)
        {
            if (length < 0 || BufferOffset > BufferSize)
            {
                throw new Exception();
            }

            length = Math.Min(length, BufferSize - BufferOffset);

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

            BufferOffset += length;
            return length;
        }

        void LoadMono(List<short> input, int length)
        {
            for (int i = 0; i < length; i++)
            {
                Buffer[BufferOffset + i] = input[i];
            }
        }

        void LoadStereo(List<short> input, int length)
        {
            for (int i = 0; i < length; i++)
            {
                Buffer[BufferOffset + i] = (short)((input[2 * i] + input[2 * i + 1]) / 2);
            }
        }

        void LoadMultiChannel(List<short> input, int length)
        {
            for (int i = 0; i < length; i++)
            {
                long sum = 0;
                for (int j = 0; j < NumChannels; j++)
                {
                    sum += input[i*NumChannels+j];
                }
                Buffer[BufferOffset + i] = (short)(sum / NumChannels);
            }
        }

        void Resample()
        {
            if (ResampleInputMemoryStream == null)
            {
                Consumer.Consume(Buffer, BufferOffset);
                BufferOffset = 0;
                return;
            }
            else
            {
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


                ///////////////
                // amikor először ideér, a Buffer még ugyanaz, mint az eredetiben
                /*using (StreamWriter sw = new StreamWriter(@"C:\Users\Zsolti\Documents\Dropbox\ubuntu\nCroma (VS)\buffer_before_resample.txt"))
                {
                    int j = 0;
                    foreach (var sh in Buffer)
                    {
                        sw.WriteLine(j + "\t" + sh);
                        j++;
                    }
                }*/
                ///////////////


                byte[] bytes = new byte[2];
                //int i = 0;
                foreach (var sh in Buffer)
                {
                    //i++;

                    /*byte b1 = (byte)(sh / 256);
                    byte b2 = (byte)(sh - b1 * 256);

                    ResampleInputMemoryStream.WriteByte(b1);
                    ResampleInputMemoryStream.WriteByte(b2);*/

                    System.Buffer.BlockCopy(new short[] { sh }, 0, bytes, 0, 2);
                    //bytes = bytes.Reverse().ToArray();
                    ResampleInputMemoryStream.Write(bytes, 0, 2);
                }


                /*
                ResampleInputMemoryStream.Position = 0;
                WaveStream sourceStream = new WaveFileReader(ResampleInputMemoryStream);
                if (sourceStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
                {
                    sourceStream = WaveFormatConversionStream.CreatePcmStream(sourceStream);
                    sourceStream = new BlockAlignReductionStream(sourceStream);
                }
                if (sourceStream.WaveFormat.SampleRate != TargetSampleRate)
                {
                    sourceStream = new WaveFormatConversionStream(ResampleOutputFormat, sourceStream);
                    sourceStream = new BlockAlignReductionStream(sourceStream);
                }
                */

                ResampleInputMemoryStream.Position = 0;
                RawSourceWaveStream ResampleInputWaveStream =
                    new RawSourceWaveStream(ResampleInputMemoryStream, ResampleInputFormat);
                //ResampleInputWaveStream.Position = 0;
                //ResamplerDmoStream conversion =
                //    new ResamplerDmoStream(stream, new WaveFormat(TargetSampleRate, NumChannels));
                WaveFormatConversionStream ResampleOutputStream =
                    new WaveFormatConversionStream(ResampleOutputFormat, ResampleInputWaveStream);
                //ResampleOutputStream.SourceToDest((int)ResampleInputStream.Length);
                

                /*
                long position = 0;
                while (ResampleOutputStream.Position < ResampleOutputStream.Length)
                {
                    //byte b1 = (byte)ResampleOutputStream.ReadByte();
                    //byte b2 = (byte)ResampleOutputStream.ReadByte();
                    
                    ResampleOutputStream.Read(bytes, 0, 2);
                    position += 2;
                    ResampleOutputStream.Seek(position, SeekOrigin.Begin);

                    //bytes = bytes.Reverse().ToArray();
                    //short sh = (short)(bytes[0] * 256 + bytes[1]);
                    short[] shorts = new short[1];
                    System.Buffer.BlockCopy(bytes, 0, shorts, 0, 2);

                    //if (position < 10000)
                    //{
                    //    Debug.WriteLineIf(bytes[0] != 0, (position - 2) + ": " + bytes[0]);
                    //    Debug.WriteLineIf(bytes[1] != 0, (position - 1) + ": " + bytes[1]); 
                    //}

                    ResampleBuffer.Add(shorts[0]);
                }
                */


                int byteCnt = ResampleOutputStream.WaveFormat.AverageBytesPerSecond;
                if (byteCnt % 2 == 1)
                {
                    byteCnt++;
                }

                byte[] bytes2 = new byte[byteCnt];


                //long position = 0;
                int read;
                while ((read = ResampleOutputStream.Read(bytes2, 0, byteCnt)) > 0)
                {
                    short[] shorts = new short[read / 2];
                    System.Buffer.BlockCopy(bytes2, 0, shorts, 0, read);

                    //position += 2;
                    //if (position < 10000)
                    //{
                    //    Debug.WriteLineIf(bytes[0] != 0, (position - 2) + ": " + bytes[0]);
                    //    Debug.WriteLineIf(bytes[1] != 0, (position - 1) + ": " + bytes[1]);
                    //}
                    //Debug.WriteLineIf(bytes[0] != 0, (position - 2) + ": " + bytes[0]);
                    //Debug.WriteLineIf(bytes[1] != 0, (position - 1) + ": " + bytes[1]);

                    ResampleBuffer.AddRange(shorts);
                }
                


                ///////////////
                // amikor először ideér az eredetiben, a ResampleBuffer
                //  4086 db 0-t tartalmaz
                ///////////////



                // Megjegyzés:
                // az egész Buffer-t resample-özzük egyszerre, nem csak annyit, amennyit az eredetiben a
                //  C++ könyvtár egyszerre tudott
                // -> jó ez?

                // TODO: jó így???
                Consumer.Consume(ResampleBuffer, ResampleBuffer.Count);
                ResampleBuffer.Clear();
                BufferOffset = 0;
                return;

                /*if (ResampleBuffer.Count > 0)
                {

                    while (ResampleBuffer.Count >= kMaxBufferSize)
                    {

                        Consumer.Consume(ResampleBuffer, kMaxBufferSize);
                        Debug.WriteLine("kMaxBufferSize: " + kMaxBufferSize);
                        ResampleBuffer.RemoveRange(0, kMaxBufferSize);
                    }
                }*/

                /*Consumer.Consume(ResampleBuffer, length);
                int remaining = BufferOffset - consumed;
                if (remaining > 0)
                {
                    Buffer.RemoveRange(0, consumed);
                }
                else if (remaining < 0)
                {
                    //DEBUG() << "Chromaprint::AudioProcessor::Resample() -- Resampling overread input buffer.\n";
                    Debug.WriteLine("nChromaprint.AudioProcessor.Resample() -- Resampling overread input buffer.\n");
                    remaining = 0;
                }
                BufferOffset = remaining;*/
            }
        }
    }
}
