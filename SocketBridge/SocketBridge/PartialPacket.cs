using System;
using System.Collections.Generic;
using System.Text;

namespace SocketBridge
{
    public class PartialPacket : Packet
    {
        protected uint numDataBytesRecvd = 0;
        public uint NumDataBytesReceived
        {
            get { return numDataBytesRecvd; }
        }

        public uint NumDataBytesRemaining
        {
            get { return this.Size - numDataBytesRecvd; }
        }

        public bool IsPacketComplete
        {
            get
            {
                return (this.NumDataBytesRemaining == 0);
            }
        }

        public bool PushDataBytes(byte[] moreDataBytes)
        {
            if (this.NumDataBytesReceived + moreDataBytes.Length > this.Size)
            {
                return false;
            }


            int offsetStart = (int)this.NumDataBytesReceived;
            Buffer.BlockCopy(moreDataBytes, 0, this.data, offsetStart, moreDataBytes.Length);

            numDataBytesRecvd += (uint)moreDataBytes.Length;

            return true;
        }

        public PartialPacket(uint _id, uint _routingKey, Packet.Type _type, uint _dataSize)
            : base(_id, _routingKey, _type, new byte[_dataSize])
        {
            
        }
    }
}
