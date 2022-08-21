using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.PixelWriters
{
    public abstract class PixelWriterBase : IDisposable
    {
        public ImageInfo ImageInfo { get; protected set; }

        ~PixelWriterBase()
        {
            this.Dispose();
        }

        public abstract void Write(Color pixel);

        public virtual void Dispose() { }
        public virtual void SetImageInfo(ImageInfo imageInfo)
        {
            this.ImageInfo = imageInfo;
        }

    }
}
