using QOISharp.PixelWriters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp
{
    public class Decoder
    {
        private Stream inputStream;
        private PixelWriterBase pixelWriter;
        private long pixelCount;
        private Color[] previouslySeenPixels = new Color[64];

        public Decoder(Stream inputStream, PixelWriterBase pixelWriter)
        {
            this.inputStream = inputStream;
            this.pixelWriter = pixelWriter;

            ReadOnlySpan<byte> magicBytes = stackalloc byte[] { (byte)'q', (byte)'o', (byte)'i', (byte)'f' };
            Span<byte> header = stackalloc byte[14];

            inputStream.Read(header);

            if (!header.Slice(0, 4).SequenceEqual(magicBytes))
                throw new InvalidDataException($"Expected 'qoif' magic bytes but got {Encoding.ASCII.GetString(header.Slice(0, 4))}.");

            var imageInfo = new ImageInfo()
            {
                Width = BytesToInt(header.Slice(4, 4)),
                Height = BytesToInt(header.Slice(8, 4)),
                Channels = header[12] == 3 ? Channels.RGB : Channels.RGBA,
                ColourSpace = header[13] == 0 ? ColourSpace.SRGBLinearAlpha : ColourSpace.SRGBLinearAlpha
            };

            this.pixelCount = imageInfo.Width * imageInfo.Height;

            pixelWriter.SetImageInfo(imageInfo);
        }

        private byte HashPixel(Color pixel) => (byte)((pixel.R * 3 + pixel.G * 5 + pixel.B * 7 + pixel.A * 11) % 64);

        private void SetPreviouslySeenPixel(Color pixel)
        {
            var hash = HashPixel(pixel);
            previouslySeenPixels[hash] = pixel;
        }

        private int BytesToInt(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != 4)
                throw new ArgumentException($"{nameof(bytes)} must be a span of length 4.");

            return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
        }

        private ChunkTag DecodeChunkTag(byte tag)
        {
            if ((ChunkTag)tag == ChunkTag.FullRGB)
                return ChunkTag.FullRGB;
            else if ((ChunkTag)tag == ChunkTag.FullRGBA)
                return ChunkTag.FullRGBA;

            var top2Bits = tag >> 6;
            return (ChunkTag)top2Bits;
        }

        public void Decode()
        {
            int WrapAdd(int amount, int addAmount)
            {
                var sum = amount + addAmount;
                if (sum < 0)
                    return 256 + sum;
                else if (sum > 255)
                    return 256 - sum;
                return sum;
            }

            Span<byte> buffer = stackalloc byte[1];
            Span<byte> secondaryBuffer = stackalloc byte[1];
            Span<byte> rgbBuffer = stackalloc byte[3];
            Span<byte> rgbaBuffer = stackalloc byte[4];

            var previousColour = Color.Black;

            var pixelsWritten = 0L;
            while (pixelsWritten < this.pixelCount)
            {
                inputStream.Read(buffer);

                var chunkTag = DecodeChunkTag(buffer[0]);

                Color currentColour = Color.Black;
                switch (chunkTag)
                {
                    case ChunkTag.FullRGBA:
                        inputStream.Read(rgbaBuffer);
                        currentColour = Color.FromArgb(rgbaBuffer[3], rgbaBuffer[0], rgbaBuffer[1], rgbaBuffer[2]);
                        break;
                    case ChunkTag.FullRGB:
                        inputStream.Read(rgbBuffer);
                        currentColour = Color.FromArgb(rgbBuffer[0], rgbBuffer[1], rgbBuffer[2]);
                        break;
                    case ChunkTag.PreviouslySeenIndex:
                        var arrayIndex = buffer[0] & 0b00111111;
                        currentColour = previouslySeenPixels[arrayIndex];
                        break;
                    case ChunkTag.RunLength:
                        var runLength = buffer[0] & 0b00111111;
                        for (int i = 0; i < runLength; i++)
                            pixelWriter.Write(previousColour);
                        pixelsWritten += runLength;
                        continue;
                    case ChunkTag.ByteDifferenceToPrevious:
                        {
                            var rDiff = ((buffer[0] >> 4) & 0b11) - 2;
                            var gDiff = ((buffer[0] >> 2) & 0b11) - 2;
                            var bDiff = (buffer[0] & 0b11) - 2;
                            var r = WrapAdd(previousColour.R, rDiff);
                            var g = WrapAdd(previousColour.G, gDiff);
                            var b = WrapAdd(previousColour.B, bDiff);
                            currentColour = Color.FromArgb(255, r, g, b);
                        }
                        break;
                    case ChunkTag.ShortDifferenceToPrevious:
                        {
                            inputStream.Read(secondaryBuffer);
                            var gDiff = (buffer[0] & 0b00111111) - 32;
                            var rDiff = ((secondaryBuffer[0] >> 4) - 8) + gDiff;
                            var bDiff = ((secondaryBuffer[0] & 0b1111) - 8) + gDiff;
                            var r = WrapAdd(previousColour.R, rDiff);
                            var g = WrapAdd(previousColour.G, gDiff);
                            var b = WrapAdd(previousColour.B, bDiff);
                            currentColour = Color.FromArgb(255, r, g, b);
                        }
                        break;
                }

                SetPreviouslySeenPixel(currentColour);
                pixelWriter.Write(currentColour);
                previousColour = currentColour;
                pixelsWritten++;
            }
        }
    }
}
