using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp
{
    public class ImageInfo
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Channels Channels { get; set; } = Channels.RGB;
        public ColourSpace ColourSpace { get; set; } = ColourSpace.SRGBLinearAlpha;

        public bool HasTransparency => Channels == Channels.RGBA;
    }

    public enum Channels
    {
        RGB,RGBA
    }

    public enum ColourSpace
    {
        SRGBLinearAlpha,
        AllChannelsLinear // Not currently supported
    }
}
