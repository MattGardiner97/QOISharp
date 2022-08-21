using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.Tests
{
    public static class Constants
    {
        public static readonly string TestImageDirectory = Path.Combine(AppContext.BaseDirectory, "TestImages");

        public static class ImageNames
        {
            public const string Artificial = "artificial.ppm";
        }
    }
}
