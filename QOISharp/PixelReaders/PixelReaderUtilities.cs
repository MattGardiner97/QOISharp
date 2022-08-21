using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.PixelReaders
{
    public static class PixelReaderUtilities
    {
        public static PixelReaderBase GetPixelReader(Stream stream)
        {
            using (var binaryReader = new BinaryReader(stream,Encoding.UTF8, true))
            {
                // Check for PPM format
                if (IsPortablePixelFormat(binaryReader))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    return new PortablePixelFormatReader(stream);

                }
            }

            throw new NotSupportedException("The supplied file format is not supported.");
        }

        private static bool IsPortablePixelFormat(BinaryReader reader)
        {
            Span<byte> magicBytes = stackalloc byte[2];
            reader.Read(magicBytes);

            if (magicBytes[0] == 'P' && magicBytes[1] == '6')
                return true;

            return false;
        }
    }
}
