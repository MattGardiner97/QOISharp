using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.PixelWriters
{
    public class NullPixelWriter : PixelWriterBase
    {
        public override void Write(Color pixel) { }
    }
}
