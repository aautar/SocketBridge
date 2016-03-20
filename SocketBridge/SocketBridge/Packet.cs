using System;
using System.Collections.Generic;
using System.Text;

namespace SocketBridge
{
    public class Packet
    {
        public enum Type
        {
            IncomingRequest = 0,
            OutgoingResult = 1,
            ErrorUnableToRoute=2,
            ErrorApplicationFailure = 3,
            KillServer = 4
        }

        public const int HEADER_SIZE = 16;

        protected uint id;
        public uint Id
        {
            get { return id; }
        }

        protected uint routingKey;
        public uint RoutingKey
        {
            get { return routingKey; }
        }

        protected Type type;
        public Type PacketType {
            get { return type; }
        }

        protected uint size;
        public uint Size
        {
            get { return size; }
        }

        protected byte[] data;
        public byte[] Data
        {
            get { return data; }
        }


        public Packet(uint _id, uint _routingKey, Type _type, byte[] _data)
        {
            id = _id;
            routingKey = _routingKey;
            type = _type;
            size = (uint)_data.LongLength;
            data = _data;
        }

        public Packet(uint _id, uint routingKey, Type _type, string _data)
        {
            byte[] stringBinData = Utf8Util.StringToUtf8Bytes(_data);

            id = _id;
            type = _type;
            size = (uint)stringBinData.LongLength;
            data = stringBinData;
        }

        public byte[] ToBytes()
        {
            //System.Net.IPAddress.HostToNetworkOrder(id)

            byte[] bId = BitConverter.GetBytes(id);
            byte[] bRoutingKey = BitConverter.GetBytes(routingKey);
            byte[] bType = BitConverter.GetBytes((int)type);
            byte[] bSize = BitConverter.GetBytes(size);

            // reverse for big-endian (network byte order)
            Array.Reverse(bId);
            Array.Reverse(bRoutingKey);
            Array.Reverse(bType);
            Array.Reverse(bSize);

            return BytesPacker.Merge(bId, bRoutingKey, bType, bSize, data);
        }
    }
}
