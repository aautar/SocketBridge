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
    public class PacketTests
    {
        [TestMethod()]
        public void PacketToBytesReturnsCorrectByteArray()
        {
            byte[] data = new byte[4] { 1, 2, 3, 4 };
            Packet p = new Packet(123, 432, Packet.Type.IncomingRequest, data);
            byte[] packetBytes = p.ToBytes();

            Assert.AreEqual(20, packetBytes.Length);

            // id bytes
            CollectionAssert.AreEqual(new byte[4] { 0, 0, 0, 123 }, new byte[4] { packetBytes[0], packetBytes[1], packetBytes[2], packetBytes[3] });

            // routing key bytes
            CollectionAssert.AreEqual(new byte[4] { 0, 0, 1, 176 }, new byte[4] { packetBytes[4], packetBytes[5], packetBytes[6], packetBytes[7] });

            // type bytes
            CollectionAssert.AreEqual(new byte[4] { 0, 0, 0, 0 }, new byte[4] { packetBytes[8], packetBytes[8], packetBytes[10], packetBytes[11] });

            // size bytes
            CollectionAssert.AreEqual(new byte[4] { 0, 0, 0, 4 }, new byte[4] { packetBytes[12], packetBytes[13], packetBytes[14], packetBytes[15] });

            // data bytes
            CollectionAssert.AreEqual(data, new byte[4] { packetBytes[16], packetBytes[17], packetBytes[18], packetBytes[19] });
        }

    }
}