using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TE2Common
{
    public class SessionInfo
    {
        public readonly Mutex Mutex = new Mutex();

        public bool IsStrict { get; set; } = false;
        public bool IsFirst { get; set; } = true;
        public byte Key { get; set; } = 0x01;

        public User Client { get; set; } = null;
    };

    public class Header
    {
        public ushort Len { get; set; }
        public ushort Cmd { get; set; }
        public ushort Seq { get; set; }
        public byte RandKey { get; set; }
        public byte Packing { get; set; }
        public byte CheckFlag { get; set; }
    };

    public class Common
    {
        public static byte MakeChecksum(byte[] esi, byte key)
        {
            int ecx = esi[0];
            int edx = esi[2];
            ecx <<= 8;
            ecx += edx;
            ecx = KeyTable.Table[(ushort)ecx];

            edx = (ushort)(key << 8);
            edx += ecx;
            byte bl = KeyTable.Table[(ushort)edx];

            ecx = esi[3];
            edx = esi[1];
            ecx <<= 8;
            ecx += edx;
            ecx = KeyTable.Table[(ushort)ecx];

            edx = esi[6];
            edx <<= 8;
            edx += ecx;
            ecx = edx;
            byte al = KeyTable.Table[(ushort)ecx];

            ecx = bl;
            ecx <<= 8;
            ecx += al;

            al = KeyTable.Table[(ushort)ecx];

            return al;
        }

        public static void UpdateKey(SessionInfo sessionInfo, int tail)
        {
            byte key = sessionInfo.Key;
            int edx = tail & 0xff;
            edx <<= 8;
            edx += key;
            byte al = KeyTable.Table[(ushort)edx];
            al += key;
            sessionInfo.Key = al;
        }
    }
}
