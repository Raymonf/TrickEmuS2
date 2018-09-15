using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class _PacketReader
    {
        private static void InternalHandle(User user, ushort opcode, byte[] dec)
        {
            switch (opcode)
            {
                // Login
                case 0x2CED:
                    Login.Handle(user, dec);
                    break;
                // Server select
                case 0x2CF1:
                    ServerSelect.Handle(user, dec);
                    break;
                default:
                    Program.logger.Warn("Unhandled packet received.");
                    break;
            }
        }

        public static void HandlePacket(User user, byte[] packet)
        {
            var i = 0;
            while (i < packet.Length)
            {
                var bytes = packet.Skip(i).ToArray();
                var len = Unpacker.Unpack(user.ClientSession, bytes);
                i += len;

                var pkt = bytes.Take(len).ToArray();
                ushort lenNoDum = BitConverter.ToUInt16(pkt, 0);
                ushort opcode = BitConverter.ToUInt16(pkt, 2);
                InternalHandle(user, opcode, pkt.Skip(9).Take(lenNoDum - 9 - 2).ToArray());
            }
        }
    }
}
