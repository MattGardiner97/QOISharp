using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.PixelWriters
{
    public class RawPixelWriter : PixelWriterBase
    {
        private byte[] writeBuffer = new byte[8];
        private FileStream outputStream;

        public RawPixelWriter(FileStream outputStream)
        {
            this.outputStream = outputStream;
        }

        public override void SetImageInfo(ImageInfo imageInfo)
        {
            base.SetImageInfo(imageInfo);

            // Width/height bytes - MSB first
            writeBuffer[0] = (byte)((imageInfo.Width >> 24) & 0xFF);
            writeBuffer[1] = (byte)((imageInfo.Width >> 16) & 0xFF);
            writeBuffer[2] = (byte)((imageInfo.Width >> 8) & 0xFF);
            writeBuffer[3] = (byte)(imageInfo.Width & 0xFF);

            writeBuffer[4] = (byte)((imageInfo.Height >> 24) & 0xFF);
            writeBuffer[5] = (byte)((imageInfo.Height >> 16) & 0xFF);
            writeBuffer[6] = (byte)((imageInfo.Height >> 8) & 0xFF);
            writeBuffer[7] = (byte)(imageInfo.Height & 0xFF);

            outputStream.Write(writeBuffer, 0, 8);
        }

        public override void Write(Color pixel)
        {
            writeBuffer[0] = pixel.A;
            writeBuffer[1] = pixel.R;
            writeBuffer[2] = pixel.G;
            writeBuffer[3] = pixel.B;
            outputStream.Write(writeBuffer, 0, 4);
        }
    }
}
