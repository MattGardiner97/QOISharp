using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp
{
    public enum ChunkTag
    {
        RunLength = 0b11,
        PreviouslySeenIndex = 0b00,
        ByteDifferenceToPrevious = 0b01,
        ShortDifferenceToPrevious = 0b10,
        FullRGB = 0b11111110,
        FullRGBA = 0b11111111
    }
}
