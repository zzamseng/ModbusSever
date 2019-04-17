using ModbusServer.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusServer.Network.Message
{
    class FC15 : IMessage
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

            // read size length is 2
            var tmp = Util.SubArray(Data, 0, 2);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(tmp);
            int bitLength = BitConverter.ToInt16(tmp, 0);

            // byte count length is 1
            var byteCount = Util.SubArray(Data, 2, 1).ElementAt(0);

            // read size + byte count = 3
            var data = Util.SubArray(Data, 3, Data.Length - 3);


            var bits = Util.ByteArrToBitArray(data, byteCount, bitLength);
            //
            var memory = LocalMemoryMap.Instance.Memory(fcCode) as BitArray;

            for(int i=startAddress, j=0; i<startAddress + bitLength;i++, j++)
                memory.Set(i, bits.Get(j));


            // response data is only read size
            Data = Util.SubArray(Data, 0, 2);

            ushort totalLength = (ushort)(UnitID.Length + FcCode.Length + 1/*readsize of length*/ + Data.Length);
            var lengtharr = BitConverter.GetBytes(totalLength);
            Array.Reverse(lengtharr);

            List<byte> packet = new List<byte>();
            packet.AddRange(TransactionID);
            packet.AddRange(ProtocolID);
            packet.AddRange(lengtharr);// change length
            packet.AddRange(UnitID);
            packet.AddRange(FcCode);
            packet.AddRange(StartAddress);
            packet.AddRange(Data);

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
