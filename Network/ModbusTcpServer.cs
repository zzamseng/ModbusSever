﻿using ModbusServer.Utils;
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

        #region Const Values
        public const int TransationIDSize = 2;
        public const int ProtocolIDSize = 2;
        public const int LengthSize = 2;
        public const int UnitIDSize = 1;
        public const int FCCodeSize = 1;
        public const int StartAddressSize = 2;

        #endregion

        private ConcurrentDictionary<byte, List<byte>> _memory;


        public ModbusTcpServer()
        {
            _memory = new ConcurrentDictionary<byte, List<byte>>();

            // Coil
            _memory.TryAdd(1, new List<byte>());
            // Discrete Inputs
            _memory.TryAdd(2, new List<byte>());
            // Holding Register
            _memory.TryAdd(3, new List<byte>());
            // Input Register
            _memory.TryAdd(4, new List<byte>());

            // init value to zero
            _memory[1].AddRange(Enumerable.Range(0, 1000).Select(_ => Convert.ToByte(0)));
            _memory[2].AddRange(Enumerable.Range(0, 1000).Select(_ => Convert.ToByte(0)));
            _memory[3].AddRange(Enumerable.Range(0, 1000).Select(_ => Convert.ToByte(0)));
            _memory[4].AddRange(Enumerable.Range(0, 1000).Select(_ => Convert.ToByte(0)));

            _memory[1][1] = 1;
            _memory[3][1] = 66;
        }

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
            //MBAP Header
            //MODBUS-TCP는 MBAP(Modbus Application Protocol)를 선두로 Function code, Data로 순으로 이루어져 있습니다.MBAP는 총 7 Byte이고 아래와 같은 내용의 Byte값을 나타냅니다.

            //Transaction ID[2Bytes] : 마스터(Client) 가 최초 0x0000값 부터 통신시작 시 1씩 증가시키며 슬래이브(Server) 는 그 값을 그대로 복사해서 사용합니다.쿼리및 응답에 대해 한쌍으로 작업이 이루어 졌는지를 확인하는 부분입니다.
            //Protocol ID [2Bytes] : 프로토콜의 ID를 나타내며 MODBUS-TCP는 0x0000의 고정값을 사용합니다.
            //Length[2Bytes] : Length 필드위치에서 프레임 마지막까지의 길이를 나타냅니다. 즉 Unit ID ~ Data끝까지의 Byte의 수를 나타냅니다.
            //Unit ID [1 Byte] : TPC/IP가 아닌 다른 통신선로의 연결되어있는 Slave를 구분하는 정보입니다. Tcpport는 0x01로 고정입니다.

            TcpClient tc = o as TcpClient;

            System.Threading.CancellationToken token;

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
                            var nbytes = await stream.ReadAsync(buff, 0, MAX_SIZE, token).ConfigureAwait(false);
                            if (nbytes > 0)
                            {
                                int index = 0;

                                // MBAP
                                var arrTransactionID = Utils.Util.SubArray(buff, index, TransationIDSize);
                                index += TransationIDSize;

                                var arrProtocolID = Utils.Util.SubArray(buff, index, ProtocolIDSize);
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
                                var arrStartAddress = Utils.Util.SubArray(buff, index, StartAddressSize);
                                index += StartAddressSize;
                                length -= StartAddressSize;

                                // length is UnitID + FCCode + StartAddress + Length(Read) / Data(Write)

                                var readOrWriteParam = Utils.Util.SubArray(buff, index, length);
                                //Data

                                Console.WriteLine($"TID: {Util.ArrayToString(arrTransactionID)}, " +
                                                  $"PID: {Util.ArrayToString(arrProtocolID)}, " +
                                                  $"Length: {Util.ArrayToString(arrLength)}, " +
                                                  $"UnitID: {Util.ArrayToString(arrUnitID)}, " +
                                                  $"FC {Util.ArrayToString(arrFcCode)}, " +
                                                  $"SA {Util.ArrayToString(arrStartAddress)}, " +
                                                  $"RL {Util.ArrayToString(readOrWriteParam)}");

                                var pacekt = MakeResponsePacket(arrTransactionID, arrProtocolID, arrUnitID, arrFcCode, arrStartAddress, readOrWriteParam);

                                if(pacekt != null)
                                    await stream.WriteAsync(pacekt, 0, pacekt.Length).ConfigureAwait(false);
                            }
                            else
                            {
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

        private byte[] MakeResponsePacket(byte[] paramTransactionID,
                                          byte[] paramProtocolID,
                                          byte[] paramUnitID, 
                                          byte[] paramFcCode, 
                                          byte[] paramStartAddress, 
                                          byte[] parmReadOrWrite)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(paramStartAddress);
            
            var fcCode = paramFcCode[0];
            int startAddress = BitConverter.ToInt16(paramStartAddress, 0);
            int data = 0;
            byte[] arrData = null;

            switch (fcCode)
            {
                case 1:     // Read Coil
                    {
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(parmReadOrWrite);

                        // because word size
                        int readSize = BitConverter.ToInt16(parmReadOrWrite, 0);
                        arrData = ProcessFC1(fcCode, startAddress, readSize);
                    }
                    break;
                case 2:     // Read Discrete Inputs
                    return null;
                    break;
                case 3:     // Read Holding Register
                    {
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(parmReadOrWrite);

                        // because word size
                        int readSize = BitConverter.ToInt16(parmReadOrWrite, 0) * 2;
                        arrData = ProcessFC3(fcCode, startAddress, readSize);

                        data = readSize;
                    }
                    break;
                case 4:     // Read Input Register
                    return null;
                    break;
                case 5:     // Write Single Coil
                    return null;
                    break;
                case 6:     // Write Single Register
                    arrData = ProcessWriteRegister(fcCode, startAddress, parmReadOrWrite);
                    break;
                case 15:    // Write Multiple Coils  
                    return null;
                    break;
                case 16:    // Write Multiple Registers
                    arrData = ProcessWriteRegister(fcCode, startAddress, parmReadOrWrite);
                    break;
            }


            ushort totalLength = (ushort)(paramUnitID.Length + paramFcCode.Length + 1/*readsize of length*/ + data);
            var lengtharr = BitConverter.GetBytes(totalLength);
            Array.Reverse(lengtharr);

            List<byte> packet = new List<byte>();
            packet.AddRange(paramTransactionID);
            packet.AddRange(paramProtocolID);
            packet.AddRange(lengtharr);// change length
            packet.AddRange(paramUnitID);
            packet.AddRange(paramFcCode);
            if (fcCode < 5)   // for only read
                packet.Add(Convert.ToByte(data));
            packet.AddRange(arrData);


            return packet.ToArray();
        }

        private byte[] ProcessFC1(byte fcCode, int startAddress, int readSize)
        {
            var memory = GetMemoryFromCode(fcCode);
            var data = Utils.Util.SubArray(memory.ToArray(), startAddress, readSize);

            return data;
        }

        private byte[] ProcessWriteRegister(byte fcCode, int startAddress, byte[] writeData)
        {
            var memory = GetMemoryFromCode(fcCode);

            if (fcCode == 16)
            {
                // writedata contain length(2byte) and Byte Count(1byte) .. but dont need it
                writeData = Util.SubArray(writeData, 3, writeData.Length - 3);
            }

            for (int i = 0;i<writeData.Length;i++)
                memory[startAddress + i] = writeData[i];

            return writeData;
        }

        private byte[] ProcessFC3(byte fcCode, int startAddress, int readSize)
        {
            var memory = GetMemoryFromCode(fcCode);
            var data = Utils.Util.SubArray(memory.ToArray(), startAddress, readSize);

            return data;
        }

        private List<byte> GetMemoryFromCode(byte fcCode)
        {
            List<byte> retValue = null;
            switch (fcCode)
            {
                case 1:
                case 5:
                case 15:
                    retValue = _memory[1];
                    break;
                case 2:
                    retValue = _memory[2];
                    break;
                case 3:
                case 6:
                case 16:
                    retValue = _memory[3];
                    break;
                case 4:
                    retValue = _memory[4];
                    break;
            }

            return retValue;
        }
    }
}