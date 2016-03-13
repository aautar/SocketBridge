using System;
using System.Collections.Generic;
using System.Text;

namespace SocketBridge
{
    public class HandlerResponse
    {
        protected byte[] data;
        public byte[] ResponseData
        {
            get { return data; }
        }

        public HandlerResponse(byte[] _data)
        {
            data = _data;
        }

        public HandlerResponse(string _responseString)
        {
            data = Utf8Util.StringToUtf8Bytes(_responseString);
        }
    }
}
