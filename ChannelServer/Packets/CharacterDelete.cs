using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class CharacterDelete
    {
        /*
         * 24 00 D9 07 00 00 E0 00 A1
         * E5 6E F7 05 00 00 00 00 38 66 30 30 62 32 30 34 65 39 38 30 30 39 39 38 00 25 06 2A 2F 57
         */
        public static void Handle(User user, byte[] bytes)
        {
            var packet = new InPacket(bytes);
            // Character ID
            // Some kind of password (NUL terminated)

            var charId = packet.ReadUInt();

            Program.logger.Debug("Delete packet received: {0}", Util.ByteToHex(bytes));
            Program.logger.Debug("Character ID to delete: {0}", charId);
            
            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "UPDATE characters SET authority = 2 WHERE id = @id AND user = @userId;";
                cmd.Parameters.AddWithValue("@id", charId);
                cmd.Parameters.AddWithValue("@userId", user.Id);
                cmd.ExecuteScalar();
            }

            user.NumChars--;

            // Success packet
            PacketBuffer del = new PacketBuffer(0x7DB, user);
            del.WriteByte(0x00);
            del.Send();
        }
    }
}
