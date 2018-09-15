using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class TimeCapsule
    {
        public static void List(User user, byte[] packet)
        {
            PacketBuffer res = new PacketBuffer(0x7F9, user);
            res.WriteByte(0x16); // Max amount of frozen characters?
            res.WriteUInt16(0); // Probably number of frozen characters
            res.Send();
        }

        // 0x07F6
        public static void GetTicketCount(User user, byte[] packet)
        {
            // 0x07F7
            PacketBuffer p = new PacketBuffer(0x7F7, user);
            p.WriteUInt32(5);
            p.Send();
        }
    }
}
