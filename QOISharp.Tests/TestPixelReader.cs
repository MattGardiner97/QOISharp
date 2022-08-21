using QOISharp.PixelReaders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.Tests
{
    public class TestPixelReader : PixelReaderBase
    {
        private Color[] pixels;
        private int currentPixelIndex = 0;

        public override ImageInfo ImageInfo { get; protected set; }

        public TestPixelReader(Color[] pixels, int width, int height)
        {
            if (pixels.Length != width * height)
                throw new ArgumentException("Number of pixels does not match supplied dimensions.");

            ImageInfo = new ImageInfo()
            {
                Width = width,
                Height = height,
                Channels = Channels.RGB,
                ColourSpace = ColourSpace.SRGBLinearAlpha
            };

            this.pixels = pixels;
        }

        public override Color GetNextPixel()
        {
            return this.pixels[currentPixelIndex++];
        }
    }
}
