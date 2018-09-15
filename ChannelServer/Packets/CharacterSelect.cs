using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class CharacterSelect
    {
        // RECEIVE:
        // 14 00 DC 07 00 00 D9 00 06
        // E3 6E F7 05 00 00 00 00 00 4D 02 56 85 5D E1 12 1C B9 F1 19
        //
        // PAULA:
        // 14 00 DC 07 00 00 3F 00 8F
        // E2 6E F7 05 00 00 00 00 00 4C 02 01 12 96 9B BF 56 34 23 DF
        public static void Handle(User user, InPacket packet)
        {
            // Read in character ID
            var charId = packet.ReadUInt();

            // NOTIFY GAMESERVER IP
            /*
             * REPLY:
             * 0x07DE
             * 3A 00 DE 07 00 00 3F 04 B4
             * E3 6E F7 05 00 00 00 00 7D 09 DF 6A 00 00 31 33 39 2E 39 39 2E 31 31 35 2E 32 32 30 00 F8 55 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             * 
             * 
             * PAULA:
             * 3A 00 DE 07 00 00 87 04 EE
             * E2 6E F7 05 00 00 00 00 80 09 3E 0A 00 00 31 33 39 2E 39 39 2E 31 31 35 2E 32 32 30 00 F8 55 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00
             * 
             */

            var gs = new PacketBuffer(0x07DE, user);

            gs.WriteUInt64(charId);

            // ?
            gs.WriteHexString("7D 09 DF 6A 00 00");

            gs.WriteString(Program.config.Server["GameIP"]/*, 15*/); // IP
            gs.WriteByte(0x00);
            gs.WriteUInt16(ushort.Parse(Program.config.Server["GamePort"])); // Port
            gs.WriteBytePad(0x00, 16);
            gs.Send();

            // NOTIFY SYSTEM SERVER
            /*
             * REPLY:
             * 0x1585
             * 2A 00 85 15 00 00 6E 04 A7
             * E3 6E F7 05 00 00 00 00 31 33 39 2E 39 39 2E 31 31 35 2E 32 32 30 00 1B 34 00 00 20 07 00 00 87 05 C0 7D 51 BF F1 E2 EA 99 FD 36 A3 59 41 1D F3 83 A9 17 30
             * 
             * 
             * PAULA:
             * 2A 00 85 15 00 00 77 04 1A
             * E2 6E F7 05 00 00 00 00 31 33 39 2E 39 39 2E 31 31 35 2E 32 32 30 00 1B 34 00 00 AE 73 00 00 80 06 54 E4 36 46 72 26 7C F5 39 62 C1 11 2D F6 8C 28 7D BF 17
             */

            if (Program.config.Server["SystemEnabled"] == "1" || Program.config.Server["SystemEnabled"].ToLower() == "true")
            {
                var ss = new PacketBuffer(0x1585, user);
                ss.WriteUInt32(charId);
                ss.WriteUInt32(0); // part of long
                ss.WriteString(Program.config.Server["SystemIP"], 15); // IP
                ss.WriteUInt16(ushort.Parse(Program.config.Server["SystemPort"])); // Port
                ss.WriteHexString("00 00 AE 73 00 00");
                ss.Send();
            }

            // Tell the client that it can connect now, with the user UID
            /*
             * REPLY:
             * 0x0003
             * 15 00 03 00 00 00 EA 04 FD
             * 06 00 63 77 F6 05 00 00 00 00
             * 
             * 
             * PAULA:
             * 15 00 03 00 00 00 7C 04 39
             * 05 00 63 77 F6 05 00 00 00 00
             */

            var msg = new PacketBuffer(0x0003, user);
            msg.WriteUInt16(0);
            msg.WriteUInt64(user.Id);
            msg.Send();
        }
    }
}
