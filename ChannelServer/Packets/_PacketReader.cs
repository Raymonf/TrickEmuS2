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
                // Received character list
                // Let login (0x7D0) handle this
                case 0x07EC:
                    break;
                // Login
                case 0x07D0:
                    Login.Handle(user, new InPacket(dec));
                    break;
                // Create character
                case 0x07D6:
                    CharacterCreate.Handle(user, dec);
                    break;
                // Delete character
                case 0x07D9:
                    CharacterDelete.Handle(user, dec);
                    break;
                // Request characters inside time capsule 
                // (Frozen)
                case 0x07F8:
                    TimeCapsule.List(user, dec);
                    break;
                // Get number of time capsule tickets
                case 0x07F6:
                    TimeCapsule.GetTicketCount(user, dec);
                    break;
                // Get number of slot expansion tickets
                case 0x0817:
                    SlotExpansion.GetExpansionTicketCount(user, dec);
                    break;
                // Expand slots
                case 0x0814:
                    SlotExpansion.ExpandSlots(user, dec);
                    break;
                // Select character
                case 0x07DC:
                    CharacterSelect.Handle(user, new InPacket(dec));
                    break;

                default:
                    Program.logger.Warn("Unhandled packet received. Opcode: {0}", Util.ByteToHex(opcode));
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
