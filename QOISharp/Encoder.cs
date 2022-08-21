using QOISharp.PixelReaders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp
{
    public class Encoder
    {
        private static readonly byte[] magicBytes = new byte[] { (byte)'q', (byte)'o', (byte)'i', (byte)'f' };
        private Color[] previouslySeenPixels = new Color[64];
        private readonly byte[] writeBuffer = new byte[4];
        private PixelReaderBase pixelReader;
        private Stream outputStream;

        public Encoder(PixelReaderBase pixelReader, Stream outputStream)
        {
            this.pixelReader = pixelReader;
            this.outputStream = outputStream;
        }

        private byte BuildByteChunk(ChunkTag tag, byte data) => (byte)(((int)tag << 6) | data);
        private short BuildShortChunk(ChunkTag tag, short data) => (short)(((int)tag << 14) | (ushort)data);

        private byte HashPixel(Color pixel) => (byte)((pixel.R * 3 + pixel.G * 5 + pixel.B * 7 + pixel.A * 11) % 64);

        private void SetPreviouslySeenPixel(Color pixel)
        {
            var hash = HashPixel(pixel);
            previouslySeenPixels[hash] = pixel;
        }

        private bool PixelIsPreviouslySeen(Color pixel, out byte previouslySeenPixelIndex)
        {
            var hash = HashPixel(pixel);
            if (previouslySeenPixels[hash] == pixel)
            {
                previouslySeenPixelIndex = hash;
                return true;
            }

            previouslySeenPixelIndex = 0;
            return false;
        }

        private bool PixelIsByteDifference(Color previousPixel, Color currentPixel, out byte encodedDiff)
        {
            encodedDiff = 0;

            if (previousPixel.A != currentPixel.A)
                return false;

            var rDiff = EncoderUtilities.GetWrappedDifference(previousPixel.R, currentPixel.R);
            var gDiff = EncoderUtilities.GetWrappedDifference(previousPixel.G, currentPixel.G);
            var bDiff = EncoderUtilities.GetWrappedDifference(previousPixel.B, currentPixel.B);

            bool OutOfRange(int diff) => diff < -2 || diff > 1;
            if (OutOfRange(rDiff) || OutOfRange(gDiff) || OutOfRange(bDiff))
                return false;

            // Apply bias to clamp values to 2 bits
            rDiff += 2;
            gDiff += 2;
            bDiff += 2;

            encodedDiff = (byte)((rDiff << 4) | (gDiff << 2) | bDiff);
            return true;
        }

        private bool PixelIsShortDifference(Color previousPixel, Color currentPixel, out short encodedDiff)
        {
            encodedDiff = 0;

            if (previousPixel.A != currentPixel.A)
                return false;

            var gDiff = EncoderUtilities.GetWrappedDifference(previousPixel.G, currentPixel.G);
            var rDiff = EncoderUtilities.GetWrappedDifference(previousPixel.R, currentPixel.R);
            var bDiff = EncoderUtilities.GetWrappedDifference(previousPixel.B, currentPixel.B);

            rDiff -= gDiff;
            bDiff -= gDiff;

            if (gDiff < -32 || gDiff > 31 || rDiff < -8 || rDiff > 7 || bDiff < -8 || bDiff > 7)
                return false;

            // Apply bias to clamps values to 6 or 4 bits
            gDiff += 32;
            rDiff += 8;
            bDiff += 8;

            encodedDiff = (short)((gDiff << 8) | (rDiff << 4) | bDiff);
            return true;
        }

        private void WriteRawPixel(Color pixel, bool hasTransparency)
        {
            if (pixelReader.ImageInfo.HasTransparency)
                WriteByte((byte)ChunkTag.FullRGBA);
            else
                WriteByte((byte)ChunkTag.FullRGB);

            WriteByte(pixel.R);
            WriteByte(pixel.G);
            WriteByte(pixel.B);

            if (hasTransparency)
                WriteByte(pixel.A);
        }

        // Stream.Write(byte) allocates a single byte everytime it is called. This can be avoided by calling the Stream.Write(Byte[], int,int) method instead
        void WriteByte(byte b)
        {
            writeBuffer[0] = b;
            outputStream.Write(writeBuffer, 0, 1);
        }

        void WriteShort(short s)
        {
            writeBuffer[0] = (byte)(s >> 8);
            writeBuffer[1] = (byte)(s & 0xFF);
            outputStream.Write(writeBuffer, 0, 2);
        }

        void WriteInt(int i)
        {
            writeBuffer[0] = (byte)(i >> 24);
            writeBuffer[1] = (byte)(i >> 16);
            writeBuffer[2] = (byte)(i >> 8);
            writeBuffer[3] = (byte)(i & 0xFF);
            outputStream.Write(writeBuffer, 0, 4);
        }

        public void Encode()
        {
            if (!outputStream.CanWrite)
                throw new ArgumentException("The supplied output stream cannot be written to.");

            outputStream.Write(magicBytes, 0, 4);
            WriteInt(pixelReader.ImageInfo.Width);
            WriteInt(pixelReader.ImageInfo.Height);
            WriteByte((byte)(pixelReader.ImageInfo.HasTransparency ? 4 : 3));
            WriteByte(0);

            var pixelsToWrite = pixelReader.ImageInfo.Width * pixelReader.ImageInfo.Height;
            var previousPixel = Color.FromArgb(255, 0, 0, 0);
            var runLength = 0;
            var isMeasuringRunLength = false;

            void WriteRunLength()
            {
                WriteByte(BuildByteChunk(ChunkTag.RunLength, (byte)runLength));
                isMeasuringRunLength = false;
                runLength = 0;
            }

            for (int currentPixelIndex = 0; currentPixelIndex < pixelsToWrite; currentPixelIndex++)
            {
                var currentPixel = pixelReader.GetNextPixel();

                if (isMeasuringRunLength && (currentPixel != previousPixel || runLength == 62))
                {
                    WriteRunLength();
                }

                if (currentPixelIndex == 0)
                {
                    WriteRawPixel(currentPixel, pixelReader.ImageInfo.HasTransparency);
                }
                else if (currentPixel == previousPixel)
                {
                    isMeasuringRunLength = true;
                    runLength++;
                }
                else if (PixelIsPreviouslySeen(currentPixel, out var previouslySeenPixelIndex))
                {
                    WriteByte(BuildByteChunk(ChunkTag.PreviouslySeenIndex, previouslySeenPixelIndex));
                }
                else if (PixelIsByteDifference(previousPixel, currentPixel, out var byteEncodedDiff))
                {
                    WriteByte(BuildByteChunk(ChunkTag.ByteDifferenceToPrevious, byteEncodedDiff));
                }
                else if (PixelIsShortDifference(previousPixel, currentPixel, out var shortEncodedDiff))
                {
                    WriteShort(BuildShortChunk(ChunkTag.ShortDifferenceToPrevious, shortEncodedDiff));
                }
                else
                {
                    WriteRawPixel(currentPixel, pixelReader.ImageInfo.HasTransparency);
                }

                SetPreviouslySeenPixel(currentPixel);

                previousPixel = currentPixel;
            }

            if (isMeasuringRunLength)
                WriteRunLength();

            // Byte stream end
            for (int i = 0; i < 7; i++)
                WriteByte((byte)0);
            WriteByte((byte)1);
        }
    }
}
