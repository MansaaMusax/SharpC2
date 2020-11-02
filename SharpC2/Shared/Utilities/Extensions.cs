using System;

namespace Shared.Utilities
{
    public static class Extensions
    {
        public static byte[] TrimBytes(this byte[] bytes)
        {
            var index = bytes.Length - 1;

            while (bytes[index] == 0)
            {
                index--;
            }

            var copy = new byte[index + 1];
            Array.Copy(bytes, copy, index + 1);

            return copy;
        }
    }
}