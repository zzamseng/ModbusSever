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
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(StartAddress);

            //var fcCode = FcCode[0];
            //int startAddress = BitConverter.ToInt16(StartAddress, 0);

            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(Data);

            //int readSize = BitConverter.ToInt16(Data, 0);

            //// read memory
            //var memories = LocalMemoryMap.Instance.Memory(fcCode);
            //List<byte> data = new List<byte>();

            //int bytePos = startAddress / 7;
            //int readByte = readSize / 8;
            //int remainBit = readSize - (readByte * 8);

            //byte bitCompare = 0;
            //if (startAddress % 7 != 0)
            //{
            //    int startBit = startAddress % 8;

            //    bitCompare = 0;
            //    for (int i = startBit; i < 8; i++)
            //    {
            //        var tmp = (byte)(1 << i);
            //        bitCompare |= tmp;
            //    }

            //    byte startByte = (byte)(memories[bytePos] & bitCompare);

            //    startByte = (byte)(startByte >> startBit);

            //    data.Add(startByte);

            //    bytePos++;
            //}


            //for (int i = bytePos; i < bytePos+readByte; i++)
            //    data.Add(memories[i]);

            //bytePos += readByte;

            //bitCompare = 0;
            //for (int i = 0; i < remainBit; i++)
            //{
            //    var tmp = (byte)(1 << i);
            //    bitCompare |= tmp;
            //}

            //byte remainByte = (byte)(memories[bytePos] & bitCompare);
            //data.Add(remainByte);
            ////

            //ushort totalLength = (ushort)(UnitID.Length + FcCode.Length + 1/*readsize of length*/ + (bytePos + 1));
            //var lengtharr = BitConverter.GetBytes(totalLength);
            //Array.Reverse(lengtharr);

            //List<byte> packet = new List<byte>();
            //packet.AddRange(TransactionID);
            //packet.AddRange(ProtocolID);
            //packet.AddRange(lengtharr);// change length
            //packet.AddRange(UnitID);
            //packet.AddRange(FcCode);
            //packet.Add(Convert.ToByte((bytePos + 1)));
            //packet.AddRange(data);

            //Packet = packet.ToArray();
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
