using NChromaprint.Enums;
using NChromaprint.helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NChromaprint.Classes
{
    public class NChromaprintContext
    {
        #region Version

        static readonly int _version_major = 0;
        static readonly int _version_minor = 7;
        static readonly int _version_patch = 0;

        public static string Version
        {
            get { return _version_major + "." + _version_minor + "." + _version_patch; }
        }

        #endregion

        ChromaprintAlgorithm _algorithm;
        Fingerprinter _fingerprinter;
        List<int> _fingerprint;


        public NChromaprintContext(ChromaprintAlgorithm algorithm = ChromaprintAlgorithm.CHROMAPRINT_ALGORITHM_TEST2)
        {
            _algorithm = algorithm;
            _fingerprinter = new Fingerprinter(new FingerprinterConfiguration(algorithm));
        }


        public bool SetOption(FingerprinterOption option, int value)
        {
            return _fingerprinter.SetOption(option, value);
        }

        public bool Start(int sampleRate, int numChannels)
        {
            return _fingerprinter.Start(sampleRate, numChannels);
        }

        public bool Feed(List<short> data)
        {
            _fingerprinter.Consume(data);
            return true;
        }

        public bool Finish()
        {
            _fingerprint = _fingerprinter.Finish();
            return true;
        }

        public string GetFingerprint()
        {
            return Base64.Base64Encode(FingerprintCompressor.CompressFingerprint(_fingerprint, (int)_algorithm));
        }

        public List<int> GetRawFingerprint()
        {
            return _fingerprint;
        }


        public List<sbyte> CompressFingerprint(List<int> uncompressedFp, int algorithm)
        {
            return FingerprintCompressor.CompressFingerprint(uncompressedFp, algorithm);
        }

        public string EncodeFingerprint(List<int> uncompressedFp, int algorithm)
        {
            var compressed = FingerprintCompressor.CompressFingerprint(uncompressedFp, algorithm);
            return Base64.Base64Encode(compressed);
        }

        public List<int> DecompressFingerprint(List<sbyte> compressedFp, ref int? algorithm)
        {
            return FingerprintDecompressor.DecompressFingerprint(compressedFp, ref algorithm);
        }

        public List<int> DecodeFingerprint(string encodedFp, ref int? algorithm)
        {
            var compressed = Base64.Base64Decode(encodedFp);
            return FingerprintDecompressor.DecompressFingerprint(compressed, ref algorithm);
        }


        public bool GetFingerprintForFile(string filePath, out string fingerprint, int maxLength = 120)
        {
            try
            {
                ProcessAllData(filePath, maxLength);
                fingerprint = GetFingerprint();
                return true;
            }
            catch (Exception)
            {
                fingerprint = "";
                return false;
            }
        }

        public bool GetRawFingerprintForFile(string filePath, out List<int> fingerprint, int maxLength = 120)
        {
            try
            {
                ProcessAllData(filePath, maxLength);

                fingerprint = GetRawFingerprint();

                return true;
            }
            catch (Exception)
            {
                fingerprint = new List<int>();
                return false;
            }
        }

        private bool ProcessAllData(string filePath, int maxLength = 120)
        {
            int sampleRate, numChannels;
            var data = IOHelper.LoadAudioFile(filePath, maxLength, out sampleRate, out numChannels);

            if (!Start(sampleRate, numChannels) ||
                !Feed(data) ||
                !Finish())
            {
                return false;
            }

            return true;
        }
    }
}
