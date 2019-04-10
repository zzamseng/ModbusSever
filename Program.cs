using ModbusServer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ModbusServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new ModbusTcpServer();

            server.Init();
        }

    }
}
