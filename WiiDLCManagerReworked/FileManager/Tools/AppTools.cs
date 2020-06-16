using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WiiDLCManagerReworked.FileManager.Tools
{
    class AppTools
    {
        /// <summary>
        /// Converts endian of unsigned short (16 bit)
        /// </summary>
        /// <param name="x">Unsigned short (16 bit)</param>
        /// <returns>Converted unsigned short</returns>
        private ushort Be16(ushort x)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] bs = BitConverter.GetBytes(x);
                Array.Reverse(bs, 0, bs.Length);
                return BitConverter.ToUInt16(bs, 0);
            }
            else
            {
                return x;
            }
        }

        /// <summary>
        /// Converts endian of unsigned integer (32 bit)
        /// </summary>
        /// <param name="x">Unsigned integer (32 bit)</param>
        /// <returns>Converted unsigned integer</returns>
        private uint Be32(uint x)
        {
            if (BitConverter.IsLittleEndian)
            {
                byte[] bs = BitConverter.GetBytes(x);
                Array.Reverse(bs, 0, bs.Length);
                return BitConverter.ToUInt32(bs, 0);
            }
            else
            {
                return x;
            }
        }

        /// <summary>
        /// Adds 0 bits to unsigned integer
        /// </summary>
        /// <param name="x">Unisgned integer</param>
        /// <param name="boundary"></param>
        /// <returns></returns>
        private uint Align(uint x, uint boundary)
        {
            if (x % boundary == 0)
                return x;
            else
                return x + boundary - (x % boundary);
        }
    }
}
