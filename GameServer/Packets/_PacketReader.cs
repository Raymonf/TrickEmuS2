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

            Program.logger.Info("Opcode: {0}. Packet: {1}", Util.ByteToHex(opcode), Util.ByteToHex(dec));
            switch (opcode)
            {
                // Login
                case 0x04:
                    Login.Handle(user, new InPacket(dec));
                    break;
                // Login, part 2
                case 0x05:
                    Login.HandleSelect2(user, dec);
                    break;
                // Movement
                case 0x18:
                    CharacterMove.Handle(user, dec);
                    break;
                // Portal usage
                case 0x1D4:
                    Portal.RequestMove(user, new InPacket(dec));
                    break;
                // Portal usage
                case 0x9C:
                    Portal.EnterMap(user, new InPacket(dec));
                    break;
                // Chat
                case 0x36:
                    CharacterChat.Handle(user, new InPacket(dec));
                    break;
                // Disconnect
                case 0x194:
                    Disconnect.Handle(user, dec);
                    break;
                default:
                    Program.logger.Warn("Unhandled packet received. Opcode: {0}", Util.ByteToHex(opcode));
                    //Program.logger.Warn("Unhandled packet: {0}", Util.ByteToHex(full));
                    Program.logger.Warn("Unhandled packet data: {0}", Util.ByteToHex(dec));
                    break;
            }
        }

        public static void HandlePacket(User user, byte[] packet)
        {
            //Program.logger.Info(user.Socket.RemoteEndPoint.ToString() + ") Packed: {0}", Util.ByteToHex(packet));
            
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
