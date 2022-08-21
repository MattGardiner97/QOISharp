using Microsoft.VisualStudio.TestTools.UnitTesting;
using QOISharp.PixelWriters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.Tests
{
    [TestClass]
    public class DecoderTests
    {
        [TestMethod]
        public void EnsureCorrectDecoding()
        {
            var encodedBytes = new byte[] { 
                0x71, 0x6f, 0x69, 0x66, 0x00, 0x00, 0x00, 0x04,
                0x00, 0x00, 0x00, 0x03, 0x03, 0x00, 0xfe, 0xff,
                0x00, 0x00, 0xc3, 0xfe, 0x80, 0x80, 0x80, 0x32, 
                0xfe, 0x80, 0x7e, 0x01, 0x5c, 0x5f, 0xab, 0x3e, 
                0x98, 0xca, 0xfe, 0x00, 0xff, 0xff, 0x00, 0x00, 
                0x00, 0x00, 0x00, 0x00, 0x00, 0x01 
            };

            var expectedPixels = new Color[]
            {
                Color.Red,
                Color.Red,
                Color.Red,
                Color.Red,
                Color.FromArgb(255, 128, 128, 128),
                Color.Red,
                Color.FromArgb(255, 128, 126, 1),
                Color.FromArgb(255, 127, 127, 255),
                Color.FromArgb(255, 126, 128, 0),
                Color.FromArgb(255, 132, 139, 17),
                Color.FromArgb(255, 128, 131, 11),
                Color.FromArgb(255, 0, 255, 255)
            };

            var pixelWriter = new MemoryPixelWriter();
            using (var memoryStream = new MemoryStream(encodedBytes))
            {
                var decoder = new Decoder(memoryStream, pixelWriter);
                decoder.Decode();
            }

            Assert.AreEqual(4, pixelWriter.ImageInfo.Width);
            Assert.AreEqual(3, pixelWriter.ImageInfo.Height);

            for (int i = 0; i < pixelWriter.Pixels.Length; i++)
                Assert.AreEqual(expectedPixels[i].ToArgb(), pixelWriter.Pixels[i].ToArgb());
        }
    }
}
