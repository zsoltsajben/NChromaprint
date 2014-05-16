using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace NChromaprint
{
    public static class IOHelper
    {
        public static List<short> LoadRawAudioFile(string path)
        {
            var data = new List<short>();

            using (var fs = File.OpenRead(path))
            {
                // 16 bites mintákból álló raw fájlt tudunk beolvasni
                if (fs.Length % 2 != 0)
                {
                    throw new NotImplementedException();
                }

                for (long i = 0; i < fs.Length / 2; i++)
                {
                    byte b1 = (byte)fs.ReadByte();
                    byte b2 = (byte)fs.ReadByte();
                    short s = (short)(b2 << 8 | b1);

                    data.Add(s);
                }
            }

            return data;
        }

        public static List<short> LoadAudioFile(string path, int maxLength, out int sampleRate, out int numChannels)
        {
            AudioFileReader reader = null;
            try
            {
                reader = new AudioFileReader(path);

                if (reader.WaveFormat.BitsPerSample == 16)
                {
                    sampleRate = reader.WaveFormat.SampleRate;
                    numChannels = reader.WaveFormat.Channels;

                    var maxShorts = maxLength * numChannels * sampleRate;

                    var buffer = new byte[Math.Min(reader.Length, maxShorts * 2)];
                    var readCnt = reader.Read(buffer, 0, buffer.Length);

                    var samples = new short[readCnt / 2];
                    Buffer.BlockCopy(buffer, 0, samples, 0, readCnt);

                    return new List<short>(samples);
                }
                else if (reader.WaveFormat.BitsPerSample == 32)
                {
                    using (var conv = new Wave32To16Stream(reader))
                    {
                        sampleRate = conv.WaveFormat.SampleRate;
                        numChannels = conv.WaveFormat.Channels;

                        var maxShorts = maxLength * numChannels * sampleRate;

                        var buffer = new byte[Math.Min(conv.Length, maxShorts * 2)];
                        var readCnt = conv.Read(buffer, 0, buffer.Length);

                        var samples = new short[readCnt / 2];
                        Buffer.BlockCopy(buffer, 0, samples, 0, readCnt);

                        return new List<short>(samples);
                    }
                    
                }
                else
                {
                    throw new NotImplementedException("Not 16 or 32 bit audio!");
                }
            }
            finally
            {
                try
                {
                    reader.Dispose();
                }
                catch { }
            }
        }
    }
}
