using System;
using System.Collections.Generic;
using System.Text;

namespace SocketBridgeAppServer
{
    public class Router : SocketBridge.IRequestHandler
    {
        public enum RoutingKey
        {
            Echo = 1
        }


        public SocketBridge.HandlerResponse Handle(uint id, uint routingKey, byte[] data)
        {
            RoutingKey rk = (RoutingKey)routingKey;

            switch (rk)
            {
                case RoutingKey.Echo:
                    return new SocketBridge.HandlerResponse(data);

            }

            throw new SocketBridge.NoHandlerFoundException();
        }
    }
}
