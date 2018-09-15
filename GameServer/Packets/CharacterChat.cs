using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class CharacterChat
    {
        public static void Handle(User user, InPacket packet)
        {
            var text = packet.ReadString();

            var chat = new PacketBuffer(0x39, user);
            chat.WriteUInt16(user.Character.EntityId);
            chat.WriteString(user.Character.Name);
            chat.WriteByte(0x00);
            chat.WriteString(text);
            chat.WriteByte(0x00);
            chat.Send();
        }
    }
}
