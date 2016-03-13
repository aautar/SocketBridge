using System;
using System.Collections.Generic;
using System.Text;

namespace SocketBridge
{
    class BytesPacker
    {
        static public byte[] BytesToBytesWithLengthPrefix(byte[] src)
        {
            byte[] lenBytes = BitConverter.GetBytes(src.Length);
            return Merge(lenBytes, src);
        }

        static public byte[] Merge(params byte[][] args)
        {
            int totalLen = 0;
            foreach (byte[] ba in args)
            {
                totalLen += ba.Length;
            }

            byte[] result = new byte[totalLen];

            int startAt = 0;
            foreach (byte[] ba in args)
            {
                ba.CopyTo(result, startAt);
                startAt += ba.Length;
            }

            return result;
        }
    }
}
