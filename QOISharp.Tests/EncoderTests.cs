using Microsoft.VisualStudio.TestTools.UnitTesting;
using QOISharp.PixelReaders;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace QOISharp.Tests
{
    [TestClass]
    public class EncoderTests
    {
        [TestMethod]
        public void EnsureNoErrors()
        {
            using (var fileStream = File.OpenRead(Path.Combine(Constants.TestImageDirectory, Constants.ImageNames.Artificial)))
            {
                using (var pixelReader = new PortablePixelFormatReader(fileStream))
                {
                    using (var outputStream = Stream.Null)
                    {
                        var encoder = new Encoder(pixelReader, outputStream);
                        encoder.Encode();
                    }
                }
            }
        }

        [TestMethod]
        public void EnsureCorrectEncoding()
        {
            var pixels = new Color[]
            {
                // Raw encoding
                Color.Red,

                // Run length
                Color.Red,
                Color.Red,
                Color.Red,

                // Raw encoding
                Color.FromArgb(255, 128, 128, 128),

                // Previously seen
                Color.Red,

                // Raw encoding
                Color.FromArgb(255, 128, 126, 1),

                // Single byte diff
                Color.FromArgb(255, 127, 127, 255),

                // Single byte diff
                Color.FromArgb(255, 126, 128, 0),

                // 2 byte diff
                Color.FromArgb(255, 132, 139, 17),

                // 2 byte diff
                Color.FromArgb(255, 128, 131, 11),

                // Raw encoding
                Color.FromArgb(255, 0, 255, 255)
            };

            int width = 4;
            int height = 3;
            var pixelReader = new TestPixelReader(pixels, width, height);

            using (var outputStream = new MemoryStream())
            {
                var encoder = new Encoder(pixelReader, outputStream);
                encoder.Encode();

                var outputBuffer = outputStream.ToArray();
                var outputSpan = outputBuffer.AsSpan();
                byte nextByte = 0;
                ChunkTag chunkTag = 0;

                var test = string.Join(',', outputBuffer.Select(i => "0x" + i.ToString("x2")));

                // Ensure header is present
                var headerBytes = outputSpan.Slice(0, 4);
                Assert.AreEqual("qoif", System.Text.Encoding.ASCII.GetString(headerBytes));
                Assert.AreEqual(width, Helpers.BytesToInt(outputSpan.Slice(4, 4)));
                Assert.AreEqual(height, Helpers.BytesToInt(outputSpan.Slice(8, 4)));
                Assert.AreEqual(3, outputSpan[12]);
                Assert.AreEqual(0, outputSpan[13]);

                // First 4 bytes will be the raw encoding of the colour red 
                Assert.AreEqual((int)ChunkTag.FullRGB, outputSpan[14]);
                Assert.AreEqual(0xFF, outputSpan[15]);
                Assert.AreEqual(0, outputSpan[16]);
                Assert.AreEqual(0, outputSpan[17]);

                // Next byte will be run length encoding of previous colour (red)
                nextByte = outputSpan[18];
                chunkTag = (ChunkTag)(nextByte >> 6);
                var runLength = nextByte & 0b00111111;
                Assert.AreEqual(ChunkTag.RunLength, chunkTag);
                Assert.AreEqual(3, runLength);

                // Next 4 bytes will be raw encoding of colour (128, 128, 128)
                Assert.AreEqual((int)ChunkTag.FullRGB, outputSpan[19]);
                Assert.AreEqual(128, outputSpan[20]);
                Assert.AreEqual(128, outputSpan[21]);
                Assert.AreEqual(128, outputSpan[22]);

                // Next byte will be an index into the array of previously seen pixels
                nextByte = outputSpan[23];
                chunkTag = (ChunkTag)(nextByte >> 6);
                Assert.AreEqual(ChunkTag.PreviouslySeenIndex, chunkTag);
                Assert.AreEqual(50, (nextByte & 0b00111111));

                // Next 4 bytes will be raw encoding of colour (253, 251, 1)
                Assert.AreEqual((int)ChunkTag.FullRGB, outputSpan[24]);
                Assert.AreEqual(128, outputSpan[25]);
                Assert.AreEqual(126, outputSpan[26]);
                Assert.AreEqual(1, outputSpan[27]);

                // Next byte will be the diff from the previous pixel
                nextByte = outputSpan[28];
                chunkTag = (ChunkTag)(nextByte >> 6);
                Assert.AreEqual(ChunkTag.ByteDifferenceToPrevious, chunkTag);
                var rDiff = ((nextByte >> 4) & 0b11) - 2;
                var gDiff = ((nextByte >> 2) & 0b11) - 2;
                var bDiff = (nextByte & 0b11) - 2;
                Assert.AreEqual(-1, rDiff);
                Assert.AreEqual(1, gDiff);
                Assert.AreEqual(-2, bDiff);

                // Next byte will be the diff from the previous pixel
                nextByte = outputSpan[29];
                chunkTag = (ChunkTag)(nextByte >> 6);
                Assert.AreEqual(ChunkTag.ByteDifferenceToPrevious, chunkTag);
                rDiff = ((nextByte >> 4) & 0b11) - 2;
                gDiff = ((nextByte >> 2) & 0b11) - 2;
                bDiff = (nextByte & 0b11) - 2;
                Assert.AreEqual(-1, rDiff);
                Assert.AreEqual(1, gDiff);
                Assert.AreEqual(1, bDiff);

                // Next 2 bytes will be the short diff from the previous pixel
                nextByte = outputSpan[30];
                var nextByte2 = outputSpan[31];
                chunkTag = (ChunkTag)(nextByte >> 6);
                Assert.AreEqual(ChunkTag.ShortDifferenceToPrevious, chunkTag);
                gDiff = (nextByte & 0b00111111) - 32; // Get least significant 6 bits
                rDiff = ((nextByte2 >> 4) & 0b1111) + gDiff - 8;
                bDiff = (nextByte2 & 0b1111) + gDiff - 8;
                Assert.AreEqual(11, gDiff);
                Assert.AreEqual(6, rDiff);
                Assert.AreEqual(17, bDiff);

                // Next 2 bytes will be the short diff from the previous pixel
                nextByte = outputSpan[32];
                nextByte2 = outputSpan[33];
                chunkTag = (ChunkTag)(nextByte >> 6);
                Assert.AreEqual(ChunkTag.ShortDifferenceToPrevious, chunkTag);
                gDiff = (nextByte & 0b00111111) - 32; // Get least significant 6 bits
                rDiff = ((nextByte2 >> 4) & 0b1111) + gDiff - 8;
                bDiff = (nextByte2 & 0b1111) + gDiff - 8;
                Assert.AreEqual(-8, gDiff);
                Assert.AreEqual(-4, rDiff);
                Assert.AreEqual(-6, bDiff);

            }
        }
    }
}