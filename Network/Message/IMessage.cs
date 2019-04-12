using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusServer.Network.Message
{
    interface IMessage
    {
        byte[] TransactionID { get; set; }
        byte[] ProtocolID { get; set; }
        byte[] DataLength { get; set; }
        byte[] UnitID { get; set; }
        byte[] FcCode { get; set; }
        byte[] StartAddress { get; set; }
        /// <summary>
        /// read size when read, writeData when write
        /// </summary>
        byte[] Data { get; set; }

        byte[] Packet { get;}

        void MakingResponsPacket();

        string PrintDebugString();
    }
}
