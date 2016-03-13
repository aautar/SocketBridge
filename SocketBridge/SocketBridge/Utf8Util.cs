using System;
using System.Collections.Generic;
using System.Text;

namespace SocketBridge
{
    class Utf8Util
    {
        static public byte[] StringToUtf8Bytes(string str)
        {
            if (str == null)
                return new byte[0];

            if (str.Length <= 0)
                return new byte[0];

            UTF8Encoding utf8enc = new UTF8Encoding();
            return utf8enc.GetBytes(str);
        }

        static public string Utf8BytesToString(byte[] utf8bytes)
        {
            return Utf8BytesToString(utf8bytes, 0, utf8bytes.Length);
        }

        static public string Utf8BytesToString(byte[] utf8bytes, int startIndex, int count)
        {
            UTF8Encoding utf8enc = new UTF8Encoding();
            return utf8enc.GetString(utf8bytes, startIndex, count);
        }
    }
}
