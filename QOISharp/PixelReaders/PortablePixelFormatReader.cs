using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.PixelReaders
{
    public class PortablePixelFormatReader : PixelReaderBase
    {
        private int bytesPerChannel;
        private Stream inputStream;
        private BinaryReader reader;

        public override ImageInfo ImageInfo { get; protected set; } = new ImageInfo();

        public PortablePixelFormatReader(Stream inputStream)
        {
            this.inputStream = inputStream;
            this.reader = new BinaryReader(inputStream, Encoding.UTF8, true);

            Span<byte> magicNumber = stackalloc byte[2];
            reader.Read(magicNumber);

            if (magicNumber[0] != 'P' && magicNumber[1] != '6')
                throw new InvalidDataException($"Invalid magic number. Expected 'P6' but was '{Encoding.ASCII.GetString(magicNumber)}'");

            reader.Read();

            int ReadInteger()
            {
                int result = reader.ReadChar() - '0';
                while (true)
                {
                    var c = reader.ReadChar();
                    if (char.IsWhiteSpace(c))
                        return result;
                    result = (result * 10) + (c - '0');
                }
            }

            this.ImageInfo.Width = ReadInteger();
            this.ImageInfo.Height = ReadInteger();
            this.ImageInfo.Channels = Channels.RGB;
            this.ImageInfo.ColourSpace = ColourSpace.SRGBLinearAlpha;
            int maxValue = ReadInteger();
            bytesPerChannel = maxValue < 256 ? 1 : 2;
        }

        public override void Dispose()
        {
            if (reader != null) 
                reader.Dispose();
        }


        public override Color GetNextPixel()
        {
            int GetChannelValue()
            {
                if (bytesPerChannel == 1)
                    return reader.ReadByte();
                return ((reader.ReadByte() << 8) | reader.ReadByte()) / 256;
            }

            var red = GetChannelValue();
            var green = GetChannelValue();
            var blue = GetChannelValue();

            return Color.FromArgb(red, green, blue);

        }
    }
}
