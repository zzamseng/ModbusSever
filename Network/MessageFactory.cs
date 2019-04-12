using ModbusServer.Network.Message;
using ModbusServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusServer.Network
{
    class MessageFactory
    {
        const bool _isDebug = false;

        #region Const Values
        private const int TransationIDSize = 2;
        private const int ProtocolIDSize = 2;
        private const int LengthSize = 2;
        private const int UnitIDSize = 1;
        private const int FCCodeSize = 1;
        private const int StartAddressSize = 2;
        #endregion

        public enum FcType { FC1 = 1, FC2, FC3, FC4, FC5, FC6, FC15 = 15, FC16 }
        public static IMessage DecodeMessage(byte[] buff)
        {
            int index = 0;

            // MBAP
            var transactionID = Utils.Util.SubArray(buff, index, TransationIDSize);
            index += TransationIDSize;

            var protocolID = Utils.Util.SubArray(buff, index, ProtocolIDSize);
            index += ProtocolIDSize;

            var arrLength = Utils.Util.SubArray(buff, index, LengthSize);
            index += LengthSize;

            if (BitConverter.IsLittleEndian)
                Array.Reverse(arrLength);
            int length = BitConverter.ToUInt16(arrLength, 0);

            var arrUnitID = Utils.Util.SubArray(buff, index, UnitIDSize);
            index += UnitIDSize;
            length -= UnitIDSize;
            // MBAP

            // FCCode
            var arrFcCode = Utils.Util.SubArray(buff, index, FCCodeSize);
            index += FCCodeSize;
            length -= FCCodeSize;
            // FCCode

            //Data
            var startAddress = Utils.Util.SubArray(buff, index, StartAddressSize);
            index += StartAddressSize;
            length -= StartAddressSize;

            // length is UnitID + FCCode + StartAddress + Length(Read) / Data(Write)
            var data = Utils.Util.SubArray(buff, index, length);

            IMessage msg = null;
            switch ((FcType)arrFcCode[0])
            {
                // Read Coil
                case FcType.FC1:
                    msg = new FC1();
                    break;
                // Read Discrete Inputs
                case FcType.FC2:
                    msg = new FC2();
                    break;
                // Read Holding Register
                case FcType.FC3:
                    msg = new FC3();
                    break;
                // Read Input Register
                case FcType.FC4:
                    msg = new FC4();
                    break;
                // Write Single Coil
                case FcType.FC5:
                    msg = new FC5();
                    break;
                // Write Single Register
                case FcType.FC6:
                    msg = new FC6();
                    break;
                // Write Multiple Coils  
                case FcType.FC15:
                    msg = new FC15();
                    break;
                // Write Multiple Registers
                case FcType.FC16:
                    msg = new FC16();
                    break;
            }

            msg.TransactionID = transactionID;
            msg.ProtocolID = protocolID;
            msg.DataLength  = arrLength;
            msg.UnitID = arrUnitID;
            msg.FcCode = arrFcCode;
            msg.StartAddress = startAddress;
            msg.Data = data;

            // must calling function below
            msg.MakingResponsPacket();

            if (!_isDebug)
                Console.WriteLine(msg.PrintDebugString());

            return msg;
        }
    }
}
