using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QOISharp.Tests
{
    [TestClass]
    public class EncoderUtilitiesTests
    {
        [TestMethod]
        public void GetWrappedDifference_Test()
        {
            Assert.AreEqual(0, EncoderUtilities.GetWrappedDifference(100, 100));
            Assert.AreEqual(1, EncoderUtilities.GetWrappedDifference(120, 121));
            Assert.AreEqual(-1, EncoderUtilities.GetWrappedDifference(121, 120));
            Assert.AreEqual(2, EncoderUtilities.GetWrappedDifference(255, 1));
            Assert.AreEqual(-2, EncoderUtilities.GetWrappedDifference(1, 255));
            Assert.AreEqual(-1, EncoderUtilities.GetWrappedDifference(0, 255));
            Assert.AreEqual(1, EncoderUtilities.GetWrappedDifference(255, 0));
        }
    }
}
