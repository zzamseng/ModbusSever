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
    }
}
