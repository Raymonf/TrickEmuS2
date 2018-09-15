using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class ServerSelect
    {
        public static void Handle(User user, byte[] packet)
        {
            var world = BitConverter.ToUInt16(packet, 0);
            var channel = BitConverter.ToUInt16(packet, 1);

            Program.logger.Debug("User selected world {0}, channel {0}", world, channel);

            PacketBuffer server = new PacketBuffer(0x2CF2, user);
            server.WriteString("127.0.0.1", 16);
            server.WriteUInt16(4006); // LoginServer port
            server.WriteUInt16(879); // Users online? not sure
            server.Send();
        }
    }
}
