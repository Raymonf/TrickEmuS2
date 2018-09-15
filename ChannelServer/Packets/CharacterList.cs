using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class CharacterList
    {
        public static void Handle(User user, byte[] packet)
        {
            var numChars = 0;

            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM characters WHERE user = @userid AND authority != 2;";
                    cmd.Parameters.AddWithValue("@userid", Methods.CleanString(user.Id.ToString()));
                    numChars = Convert.ToInt32(cmd.ExecuteScalar());
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
                return;
            }

            user.NumChars = numChars;

            PacketBuffer chars = new PacketBuffer(0x7D2, user);

            chars.WriteByte((byte)numChars); // Number of characters

            if (numChars > 0)
            {
                byte slotNum = 0;
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM characters WHERE user = @userid AND authority != 2 LIMIT 12;";
                    cmd.Parameters.AddWithValue("@userid", Methods.CleanString(user.Id.ToString()));
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            chars.WriteUInt32(reader.GetUInt32("id")); // Character ID
                            chars.WriteInt32(0); // probably part of int64?

                            chars.WriteByte(slotNum); // Index

                            // 16 bytes maximum, but 20 is fine
                            var name = Methods.Utf8Convert(reader.GetString("name"));
                            chars.WriteString(name, 20);

                            // Job(1)
                            // 0x09 = (int)9 : Paula
                            chars.WriteUInt16(reader.GetUInt16("job"));

                            // ?
                            // Display job?
                            chars.WriteUInt16(0);

                            // FType (power/magic/sense/charm)
                            chars.WriteByte((byte)reader.GetInt32("ftype")); // (byte)Constants.CharacterTypes[9]);

                            // Type(2)
                            // 0x09 = (int)9 : Paula
                            chars.WriteByte((byte)reader.GetInt32("type")); // 0x09

                            // ?
                            // [Dev]Raymonf = 01 0C
                            chars.WriteHexString("00 01");

                            // hair color
                            // 0x0A
                            chars.WriteUInt16((byte)reader.GetUInt16("hair"));

                            // Galders
                            chars.WriteUInt32((byte)reader.GetUInt32("money"));

                            // Level
                            chars.WriteUInt16(reader.GetUInt16("level"));

                            // Hat (head item)
                            // Rookie Hat
                            chars.WriteUInt32(4030);

                            // Weapon (sword)
                            // Rookie Sword
                            chars.WriteUInt32(3030);

                            // Shield
                            // Rookie Shield
                            //chars.WriteUInt(6530);
                            chars.WriteUInt32(0); // paula can't wear these

                            // Innerwear
                            chars.WriteUInt32(0);

                            // Accessory 1
                            // Duckling Keychain
                            chars.WriteUInt32(19241);

                            // Bear's ears
                            chars.WriteInt32(6932);

                            // Bear's tail
                            chars.WriteInt32(6982);

                            // Drill
                            // Flicker Drill
                            chars.WriteUInt32(19901);

                            // Pet
                            // Peng
                            chars.WriteUInt32(1642);

                            // Accessory 2
                            // Tin tiger amulet
                            chars.WriteUInt32(440004);


                            //////// Unchartered territories
                            // ??
                            chars.WriteUInt32(0);

                            // ??
                            chars.WriteUInt32(0);

                            // ??
                            chars.WriteUInt32(0);

                            // ???
                            chars.WriteUInt32(0);

                            // ???
                            chars.WriteUInt32(0);

                            // ???
                            chars.WriteUInt32(0);

                            // ???
                            chars.WriteUInt32(0);

                            // ???
                            chars.WriteUInt32(0);

                            // Ammo?
                            chars.WriteUInt32(5500);

                            // Cape
                            chars.WriteUInt32(35082);

                            // Sprint
                            chars.WriteUInt32(888887);

                            // TODO
                            // ?
                            chars.WriteHexString("02 01 00");

                            // 3rd job
                            chars.WriteUInt16(22);
                            chars.WriteUInt16(29);

                            chars.WriteHexString("00 01 00 00 00 00");
                            //chars.WriteByte(0x00);
                            chars.WriteUInt32(0);
                            chars.WriteUInt32(0);
                            chars.WriteUInt32(0);
                            chars.WriteHexString("00 00");

                            slotNum++;
                        }
                    }
                }
            }

            chars.WriteHexString("00 00 00 00 00 00 00 00 00 00 00");
            chars.WriteByte((byte)user.SlotsAllowed); // Number of character slots

            Program.logger.Debug("Character select packet: {0}", Util.ByteToHex(chars.GetBuffer()));

            chars.Send();
        }
    }
}
