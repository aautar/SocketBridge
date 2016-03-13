using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace SocketBridge
{
    public class Server
    {
        const int MAX_PACKET_SIZE = 1048510;

        public enum ResultCode
        {
            NoError,
            FailedToReadPacketData,
            FailedToParseJSONRequest,
            UnknownPacketType,
            UnknownFunction
        }
        
        protected Thread primaryExecutionThread = null;
        protected AutoResetEvent waitHandle = new AutoResetEvent(false);
        protected int exInterval = 100;
        protected bool primaryExecutionThreadKillSignal = false;

        protected TcpListener listener = null;

        protected List<Socket> clients = new List<Socket>();
        protected Dictionary<Socket, PartialPacket> clientToIncomingPartialPacket = new Dictionary<Socket, PartialPacket>();

        protected Logger logger = null;
        protected IRequestHandler packetRouter = null;

        public Server(int _port, Logger _logger, IRequestHandler _packetRouter, string _ipAddress = "127.0.0.1")
        {
            IPAddress localAddr = IPAddress.Parse(_ipAddress);
            listener = new TcpListener(localAddr, _port);

            logger = _logger;

            packetRouter = _packetRouter;

            logger.logInfo("Initializing SocketBridge server", new[] { 
                
                new KeyValuePair<string, string>("port", _port.ToString()),
                new KeyValuePair<string, string>("ipAddress", _ipAddress),
                new KeyValuePair<string, string>("version", "0.02"),
            
            });

            ThreadPool.SetMaxThreads(16,16);
        }

        public void Start()
        {
            listener.Start(10);

            primaryExecutionThread = new Thread(new ThreadStart(Process));
            primaryExecutionThread.Start();

            logger.logInfo("SocketBridge server started, listening for connections");
        }

        public void Join()
        {
            logger.logInfo("Executing join on SocketBridge server's primary execution thread");
            primaryExecutionThread.Join();
        }

        protected void ProcessClientCommand(Socket clientSock, Packet incomingPckt)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object state)
            {
                Packet outgoingPacket = null;

                try
                {
                    HandlerResponse hr = packetRouter.Handle(incomingPckt.Id, incomingPckt.RoutingKey, incomingPckt.Data);

                    outgoingPacket = new Packet(incomingPckt.Id, incomingPckt.RoutingKey, Packet.Type.OutgoingResult, hr.ResponseData);
                    clientSock.Send(outgoingPacket.ToBytes());
                }
                catch (NoHandlerFoundException)
                {
                    outgoingPacket = new Packet(incomingPckt.Id, incomingPckt.RoutingKey, Packet.Type.ErrorUnableToRoute, new byte[0]);
                    clientSock.Send(outgoingPacket.ToBytes());
                }

                logger.logInfo("Outgoing packet sent", new[] {                 
                    new KeyValuePair<string, string>("client_id", clientSock.GetHashCode().ToString()),
                    new KeyValuePair<string, string>("packet_id", outgoingPacket.Id.ToString()),            
                    new KeyValuePair<string, string>("packet_routing_key", outgoingPacket.RoutingKey.ToString()),         
                    new KeyValuePair<string, string>("packet_type", outgoingPacket.PacketType.ToString())
                });

            }), null);

            logger.logInfo("Incoming packet sent for processing", new[] {                 
                    new KeyValuePair<string, string>("client_id", clientSock.GetHashCode().ToString()),
                    new KeyValuePair<string, string>("packet_id", incomingPckt.Id.ToString()),            
                    new KeyValuePair<string, string>("packet_routing_key", incomingPckt.RoutingKey.ToString()),          
                    new KeyValuePair<string, string>("packet_type", incomingPckt.PacketType.ToString())
                });

        }

        protected void HandleNewClients()
        {
            if (listener.Pending())
            {
                Socket newClient = listener.AcceptSocket();
                clients.Add(newClient);

                logger.logInfo("Accepting new client", new[] {                 
                    new KeyValuePair<string, string>("client_id", newClient.GetHashCode().ToString()),
                    new KeyValuePair<string, string>("endpoint", newClient.LocalEndPoint.ToString())            
                });
            }
        }


        protected PartialPacket ReadIncomingPartialPacket(Socket client)
        {
            byte[] buf = new byte[Packet.HEADER_SIZE];

            int numBytesRecv = client.Receive(buf);
            if (numBytesRecv != Packet.HEADER_SIZE)
            {
                // wtf client.Available?!
                logger.logError("Couldn't receive the bytes necessary to build packet header, dropping client");
                clients.Remove(client);
                return null;
            }

            uint packetId = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf, 0));
            uint packetRoutingKey = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf, 4));
            int packetType = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf, 8));
            uint packetSize = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buf, 12));

            if (packetSize > MAX_PACKET_SIZE)
            {
                logger.logError("Packet too large, dropping client", new[] {                 
                                    new KeyValuePair<string, string>("client_id", client.GetHashCode().ToString())
                            });

                clients.Remove(client);
                return null;
            }

            PartialPacket incomingPacket = new PartialPacket(packetId, packetRoutingKey, (Packet.Type)packetType, packetSize);

            return incomingPacket;
        }

        protected void HandleIncomingPacketsForAcceptedClients()
        {
            var availableClients = new Socket[clients.Count];
            clients.CopyTo(availableClients);

            foreach (Socket client in availableClients)
            {
                try
                {
                    int numBytesAvailable = client.Available;
                    if (numBytesAvailable == 0)
                    {
                        continue;
                    }

                    if (numBytesAvailable >= Packet.HEADER_SIZE)
                    {
                        PartialPacket incomingPacket = ReadIncomingPartialPacket(client);
                        clientToIncomingPartialPacket[client] = incomingPacket;
                    }

                    bool hasIncomingPacket = clientToIncomingPartialPacket.ContainsKey(client);
                    if (hasIncomingPacket)
                    {
                        PartialPacket incomingPartialPacket = clientToIncomingPartialPacket[client];

                        byte[] incomingBuffer = new byte[Math.Min(numBytesAvailable, incomingPartialPacket.NumDataBytesRemaining)];
                        int bytesRecv = client.Receive(incomingBuffer, 0, incomingBuffer.Length, SocketFlags.None);

                        incomingPartialPacket.PushDataBytes(incomingBuffer);

                        if (incomingPartialPacket.IsPacketComplete)
                        {
                            Packet newPacket = incomingPartialPacket;
                            clientToIncomingPartialPacket.Remove(client);
                            ProcessClientCommand(client, newPacket);
                        }

                    }

                }
                catch (Exception)
                {
                    clients.Remove(client);
                }
            }
        }


        protected void Process()
        {
            while (!primaryExecutionThreadKillSignal)
            {
                try
                {
                    HandleNewClients();
                    HandleIncomingPacketsForAcceptedClients();

                    waitHandle.WaitOne(exInterval, false);
                }
                catch (Exception ex)
                {
                    logger.logError("Unhandled exception on SocketBridge server primary execution thread", new[] {                 
                        new KeyValuePair<string, string>("exception", ex.ToString())            
                    });
                }
            }
        }

    }
}
