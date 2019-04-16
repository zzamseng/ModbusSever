using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusServer.Utils
{
    class Util
    {
        public static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);

            return result;
        }

        public static string ArrayToString<T>(T[] data)
        {
            string result = "";

            foreach (var tmp in data)
            {
                result += tmp;
                result += " ";
            }

            return result;
        }

        public static byte[] BitArrayToByteArr(System.Collections.BitArray bits, int index, int length)
        {
            int numBytes = length / 8;
            if (length % 8 != 0) numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0;
            int bitIndex = 0;

            for (int i = index; i < length; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << bitIndex);

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }
    }
}
