using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.PixelReaders
{
    public abstract class PixelReaderBase : IDisposable
    {
        public abstract ImageInfo ImageInfo { get; protected set; }

        ~PixelReaderBase()
        {
            this.Dispose();
        }

        public abstract Color GetNextPixel();

        public virtual void Dispose() { }
    }
}
