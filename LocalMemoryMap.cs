using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ModbusServer.Network.MessageFactory;

namespace ModbusServer
{
    class LocalMemoryMap
    {
        private static ConcurrentDictionary<byte, List<byte>> _memory = new ConcurrentDictionary<byte, List<byte>>();

        private static LocalMemoryMap _instance;
        public static LocalMemoryMap Instance => _instance ?? (_instance = new LocalMemoryMap());

        private LocalMemoryMap()
        {
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

            // Test 
            _memory[1][0] = 5;
            _memory[3][1] = 66;
            //
        }

        public List<byte> Memory(byte type)
        {
            List<byte> retValue = null;
            switch ((FcType)type)
            {
                case FcType.FC1:
                case FcType.FC5:
                case FcType.FC15:
                    retValue = _memory[1];
                    break;
                case FcType.FC2:
                    retValue = _memory[2];
                    break;
                case FcType.FC3:
                case FcType.FC6:
                case FcType.FC16:
                    retValue = _memory[3];
                    break;
                case FcType.FC4:
                    retValue = _memory[4];
                    break;
            }

            return retValue;
        }
    }
}