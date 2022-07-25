using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncryptWithOTP
{
    static class Converter
    {
        public static byte[] GetBytes(bool[] b)
        {
            byte[] by = new byte[b.Length / 8];

            for (int i = 0; i < b.Length; i += 8)
            {
                bool[] acht = new bool[8];

                for (int j = 0; j < 8; j++)
                {
                    acht[j] = b[i + j];
                }

                by[i / 8] = GetByte(acht);
            }
            return by;
        }

        public static byte GetByte(bool[] bits)
        {
            int result = 0;
            for (int i = 0; i < bits.Length; i++)
                if (bits[i])
                    result |= 1 << i;
            return (byte)result;
        }

        public static bool[] GetBools(byte[] by)
        {
            bool[] b = new bool[by.Length * 8];
            int result = 0;
            int[] masks = { 0b00000001, 0b00000010, 0b00000100, 0b00001000, 0b00010000, 0b00100000, 0b01000000, 0b10000000 };

            for (int i = 0; i < b.Length; i += 8)
            {
                for (int j = 0; j < 8; j++)
                {
                    result = by[i / 8] & masks[j];

                    if (result != 0) //gesetzes Bit 
                    {
                        b[i + j] = true;
                    }
                    else
                    {
                        b[i + j] = false;
                    }
                }
            }
            return b;
        }
    }
}
