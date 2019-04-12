using ModbusServer.Network.Message;
using ModbusServer.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ModbusServer.Network
{
    class ModbusTcpServer
    {
        public const int ModbusTcpPort = 502;


        public void Init()
        {
            Console.WriteLine("###############################################");
            Console.WriteLine("############## Modbus TCP Server ##############");
            Console.WriteLine("###############################################");
            AysncServer().Wait();
        }

        async Task AysncServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, ModbusTcpPort);
            listener.Start();
            while (true)
            {
                TcpClient tc = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                tc.ReceiveTimeout = 500;
                tc.SendTimeout = 500;
                Console.WriteLine($"Connect client: {((IPEndPoint)tc.Client.RemoteEndPoint).Address.ToString()}");

                var task = Task.Factory.StartNew(AsyncTcpProcess, tc);
            }
        }

        async  void AsyncTcpProcess(object o)
        {
            TcpClient tc = o as TcpClient;

            if (tc != null)
            {
                int MAX_SIZE = 1024;
                using (NetworkStream stream = tc.GetStream())
                {
                    var buff = new byte[1024];

                    while (true)
                    {
                        try
                        {
                            if (!tc.Connected)
                            {
                                Console.WriteLine($"disconnected client: {((IPEndPoint)tc.Client.RemoteEndPoint).Address.ToString()}");
                                break;
                            }

                            // ~ length (MBap)
                            var nbytes = await stream.ReadAsync(buff, 0, MAX_SIZE).ConfigureAwait(false);
                            if (nbytes > 0)
                            {
                                IMessage msg = MessageFactory.DecodeMessage(buff);

                                if(msg != null)
                                    await stream.WriteAsync(msg.Packet, 0, msg.Packet.Length).ConfigureAwait(false);
                            }
                            else
                            {
                                // End Stream or Close Stream
                                Console.WriteLine($"disconnected client: {((IPEndPoint)tc.Client.RemoteEndPoint).Address.ToString()}");
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    tc.Close();
                }
            }
        }
    }
}