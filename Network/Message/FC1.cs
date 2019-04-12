using ModbusServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusServer.Network.Message
{
    class FC1 : IMessage
    {
        public byte[] TransactionID { get; set; }
        public byte[] ProtocolID { get; set; }
        public byte[] DataLength { get; set; }
        public byte[] UnitID { get; set; }
        public byte[] FcCode { get; set; }
        public byte[] StartAddress { get; set; }
        public byte[] Data { get; set; }


        public byte[] Packet { get; private set; }

        public void MakingResponsPacket()
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(StartAddress);

            var fcCode = FcCode[0];
            int startAddress = BitConverter.ToInt16(StartAddress, 0);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(Data);

            int readSize = BitConverter.ToInt16(Data, 0);

            // read memory
            var memories = LocalMemoryMap.Instance.Memory(fcCode);
            List<byte> data = new List<byte>();

            int byteCnt = readSize / 8;
            int remainBit = readSize - (byteCnt * 8);

            for (int i = startAddress; i < startAddress + byteCnt; i++)
                data.Add(memories[i]);

            byte bitCompare = 0;
            for (int i = 0; i < remainBit; i++)
            {
                var tmp = (byte)(1 << i);
                bitCompare |= tmp;
            }

            byte remainByte = (byte)(memories[startAddress + byteCnt] & bitCompare);
            data.Add(remainByte);
            //

            ushort totalLength = (ushort)(UnitID.Length + FcCode.Length + 1/*readsize of length*/ + readSize);
            var lengtharr = BitConverter.GetBytes(totalLength);
            Array.Reverse(lengtharr);

            List<byte> packet = new List<byte>();
            packet.AddRange(TransactionID);
            packet.AddRange(ProtocolID);
            packet.AddRange(lengtharr);// change length
            packet.AddRange(UnitID);
            packet.AddRange(FcCode);
            packet.Add(Convert.ToByte(readSize));
            packet.AddRange(data);

            Packet = packet.ToArray();
        }

        public string PrintDebugString()
        {
            string debug = $"TID: {Util.ArrayToString(TransactionID)}, " +
                           $"PID: {Util.ArrayToString(ProtocolID)}, " +
                           $"Length: {Util.ArrayToString(DataLength)}, " +
                           $"UnitID: {Util.ArrayToString(UnitID)}, " +
                           $"FC {Util.ArrayToString(FcCode)}, " +
                           $"SA {Util.ArrayToString(StartAddress)}, " +
                           $"RL {Util.ArrayToString(Data)}";

            return debug;
        }
    }
}
