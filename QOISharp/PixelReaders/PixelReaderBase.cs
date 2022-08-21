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
        //protected Stream inputStream;
        //protected BinaryReader reader;

        public abstract ImageInfo ImageInfo { get; protected set; }

        ~PixelReaderBase()
        {
            this.Dispose();
        }

        //public PixelReaderBase(Stream inputStream)
        //{
        //    this.inputStream = inputStream;
        //    this.reader = new BinaryReader(inputStream,Encoding.UTF8, true);
        //}

        public abstract Color GetNextPixel();

        public virtual void Dispose() { }
    }
}
