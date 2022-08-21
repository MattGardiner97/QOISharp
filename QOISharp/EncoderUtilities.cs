using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp
{
    public static class EncoderUtilities
    {
        public static int GetWrappedDifference(int from, int to, int max = 256)
        {
            if (from == to)
                return 0;

            var toFromDiff = to - from;
            var fromToDiff = from - to;

            if (fromToDiff < 0)
                fromToDiff += max;
            else if (toFromDiff < 0)
                toFromDiff += max;

            if(fromToDiff <= toFromDiff)
            {
                if (to - from > 0)
                    return to - from - max;
                return to - from;
            }
            else
            {
                if (to - from < 0)
                    return to - from + max;
                return to - from;
            }
        }
    }
}
