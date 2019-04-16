using System;
using System.Collections;
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
        private static ConcurrentDictionary<byte, List<byte>> _wordMemory = new ConcurrentDictionary<byte, List<byte>>();
        private static ConcurrentDictionary<byte, BitArray> _bitMemory = new ConcurrentDictionary<byte, BitArray>();

        private static LocalMemoryMap _instance;
        public static LocalMemoryMap Instance => _instance ?? (_instance = new LocalMemoryMap());

        private LocalMemoryMap()
        {
            // Coil
            _bitMemory.TryAdd(1, new BitArray(1000));
            // Discrete Inputs
            _bitMemory.TryAdd(2, new BitArray(1000));
            // Holding Register
            _wordMemory.TryAdd(3, new List<byte>());
            // Input Register
            _wordMemory.TryAdd(4, new List<byte>());

            // init value to zero
            _wordMemory[3].AddRange(Enumerable.Range(0, 1000).Select(_ => Convert.ToByte(0)));
            _wordMemory[4].AddRange(Enumerable.Range(0, 1000).Select(_ => Convert.ToByte(0)));

            // Test 
            //_memory[1][0] = 5;
            _wordMemory[3][1] = 66;
            //
        }

        public object Memory(byte type)
        {
            object retValue = null;
            switch ((FcType)type)
            {
                case FcType.FC1:
                case FcType.FC5:
                case FcType.FC15:
                    retValue = _bitMemory[1];
                    break;
                case FcType.FC2:
                    retValue = _bitMemory[2];
                    break;
                case FcType.FC3:
                case FcType.FC6:
                case FcType.FC16:
                    retValue = _wordMemory[3];
                    break;
                case FcType.FC4:
                    retValue = _wordMemory[4];
                    break;
            }

            return retValue;
        }
    }
}