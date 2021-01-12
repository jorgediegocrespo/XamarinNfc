using System;
using System.Linq;

namespace AccessControl.Extensions
{
    public static class StringExtensions
    {
        public static byte[] StringToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string GetReverseHex(this string hex)
        {
            string result = string.Empty;
            for (int i = 0; i < hex.Length; i = i + 2)
            {
                string hexByte = hex.Substring(i, 2);
                result = hexByte + result;
            }

            return result;
        }
    }
}
