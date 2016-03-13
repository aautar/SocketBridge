using System;
using System.Collections.Generic;
using System.Text;

namespace SocketBridgeAppServer
{
    class SocketBridgeAppServer
    {
        static void Main(string[] args)
        {
            SocketBridge.Server server = new SocketBridge.Server(54322, 
                                                                 new SocketBridge.Logger(), 
                                                                 new Router(), 
                                                                 "127.0.0.1");
            server.Start();
            server.Join();
        }
    }
}
