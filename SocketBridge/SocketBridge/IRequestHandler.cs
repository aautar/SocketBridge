using System;
using System.Collections.Generic;
using System.Text;

namespace SocketBridge
{
    public interface IRequestHandler
    {
        HandlerResponse Handle(uint id, uint routingKey, byte[] data);
    }
}
