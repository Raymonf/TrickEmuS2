using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class SlotExpansion
    {
        // Recv 0x0817
        public static void GetExpansionTicketCount(User user, byte[] packet)
        {
            // Send 0x0818
            PacketBuffer p = new PacketBuffer(0x0818, user);
            p.WriteUInt32((uint)user.SlotTickets);
            p.Send();
        }

        // Recv 0x0814
        public static void ExpandSlots(User user, byte[] packet)
        {
            // Send 0x0815
            PacketBuffer p = new PacketBuffer(0x0815, user);
            p.WriteUInt32((uint)++user.SlotsAllowed); // New number of character slots
            p.Send();

            // Subtract tickets and notify client
            user.SlotTickets--;
            GetExpansionTicketCount(user, packet);
        }
    }
}
