using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.Tests
{
    public static class Helpers
    {
        public static int BytesToInt(Span<byte> bytes)
        {
            var result = 0;
            for (int i = 0; i < bytes.Length; i++)
                result = (result << 8) | bytes[i];
            return result;
        }

        public static int BytesToInt(byte[] bytes) => BytesToInt(bytes.AsSpan());

    }
}
