using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NChromaprint.helper
{
    public static class Base64
    {
        public static string Base64Encode(List<sbyte> original)
        {
            var bytes = (byte[])(object)original.ToArray();
            var base64 = System.Convert.ToBase64String(bytes);

            // speciális Base 64-et használ a Chromaprint
            return base64.Replace("+", "-").Replace("/", "_").Replace("=", "");
        }

        public static List<sbyte> Base64Decode(string encoded)
        {
            // speciális Base 64-et használ a Chromaprint
            encoded = encoded.Replace("-", "+").Replace("_", "/");
            while (encoded.Length % 4 != 0)
                encoded += "=";

            var original = System.Convert.FromBase64String(encoded);
            var sbytes = (sbyte[])(object)original;
            return new List<sbyte>(sbytes);
        }
    }
}
