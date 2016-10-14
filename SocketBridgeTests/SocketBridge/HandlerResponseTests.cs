using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocketBridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketBridge.Tests
{
    [TestClass()]
    public class HandlerResponseTests
    {
        [TestMethod()]
        public void HandlerResponseSetsByteArray()
        {
            byte[] data = new byte[3] { 1, 2, 3 };
            HandlerResponse hr = new HandlerResponse(data);
            Assert.AreEqual(data, hr.ResponseData);
        }

        [TestMethod()]
        public void HandlerResponseSetsUtf8BytesFromString()
        {
            HandlerResponse hr = new HandlerResponse("😀");

            Assert.AreEqual(4, hr.ResponseData.Length);
            Assert.AreEqual(240, hr.ResponseData[0]);
            Assert.AreEqual(159, hr.ResponseData[1]);
            Assert.AreEqual(152, hr.ResponseData[2]);
            Assert.AreEqual(128, hr.ResponseData[3]);
        }
    }
}