using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.PixelWriters
{
    public class MemoryPixelWriter : PixelWriterBase
    {
        public Color[] Pixels { get; private set; }

        private int currentIndex = 0;

        public override void SetImageInfo(ImageInfo imageInfo)
        {
            base.SetImageInfo(imageInfo);
            Pixels = new Color[imageInfo.Width * imageInfo.Height];
        }

        public override void Write(Color pixel)
        {
            Pixels[currentIndex++] = pixel;
        }
    }
}
