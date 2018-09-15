using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class CharacterCreate
    {
        public static void Handle(User user, byte[] packet)
        {
            Program.logger.Debug("Received packet: {0}", Util.ByteToHex(packet));

            var nameBytes = Methods.ReadTerminatedStringToBytes(packet);

            var startIdx = nameBytes.Length + 1;

            byte type = packet[startIdx]; // 0x09
            byte job = packet[startIdx + 3]; // 0x09

            byte hairColor = packet[startIdx + 2]; // 0x0A - hair color

            int stat_p = packet[startIdx + 5];
            int stat_m = packet[startIdx + 6];
            int stat_s = packet[startIdx + 7];
            int stat_c = packet[startIdx + 8];

            var name = Constants.Encoding.GetString(nameBytes);
            
            // Check the name
            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM characters WHERE name LIKE @name AND authority != 2;";
                    cmd.Parameters.AddWithValue("@name", Methods.CleanString(name));

                    if (Convert.ToInt32(cmd.ExecuteScalar()) >= 1)
                    {
                        var fail = new PacketBuffer(0x7D7, user);
                        fail.WriteUInt32(0x3FF);
                        fail.Send();

                        cmd.Dispose();
                        return;
                    }

                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
                return;
            }


            uint newCharId = 0;

            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "INSERT INTO characters (user, name, job, type, ftype, hair, build) VALUES (@userid, @charname, @job, @type, @ftype, @hair, @build); select last_insert_id();";
                cmd.Parameters.AddWithValue("@userid", user.Id);
                cmd.Parameters.AddWithValue("@charname", name);
                cmd.Parameters.AddWithValue("@job", job);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@ftype", Constants.CharacterTypes[type]);
                cmd.Parameters.AddWithValue("@hair", hairColor);
                cmd.Parameters.AddWithValue("@build", stat_p + "," + stat_m + "," + stat_s + "," + stat_c);
                newCharId = Convert.ToUInt32(cmd.ExecuteScalar());
            }
            
            Program.logger.Debug("New character ID: {0}", newCharId);

            user.NumChars++;

            PacketBuffer res = new PacketBuffer(0x7D8, user);
            
            res.WriteUInt32(newCharId); // Character ID
            res.WriteInt32(0); // probably part of int64?

            res.WriteByte((byte)(user.NumChars - 1)); // Index

            // 16 bytes maximum, but 20 is fine
            res.WriteString(name, 20);

            // Job(1)
            // 0x09 = (int)9 : Paula
            res.WriteUInt16(job);

            // ?
            // Display job?
            res.WriteUInt16(0);

            // FType (power/magic/sense/charm)
            res.WriteByte((byte)Constants.CharacterTypes[type]);

            // Type(2)
            // 0x09 = (int)9 : Paula
            res.WriteByte((byte)type); // 0x09

            // ?
            // [Dev]Raymonf = 0C 03
            res.WriteHexString("00 01");

            // hair color
            // 0x0A
            res.WriteUInt16(hairColor);

            // Galders
            res.WriteUInt32(0);

            // Level
            res.WriteUInt16(1);

            // Hat (head item)
            // Rookie Hat
            res.WriteUInt32(0);

            // Weapon (sword)
            // Rookie Sword
            res.WriteUInt32(0);

            // Shield
            // Rookie Shield
            res.WriteUInt32(0);

            // ??
            res.WriteUInt32(0);

            // Accessory 1
            // Jeweled Egg 40
            res.WriteUInt32(0);

            // Bear's ears
            res.WriteInt32(6932);

            // Bear's tail
            res.WriteInt32(6982);

            // Flicker Drill
            res.WriteUInt32(0);

            // ??
            res.WriteUInt32(0);

            // Accessory 2
            // Pocket pouch
            res.WriteUInt32(7000);

            // ??
            res.WriteUInt32(0);

            // ??
            res.WriteUInt32(0);

            // ??
            res.WriteUInt32(0);

            // TODO
            res.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

            Program.logger.Debug("Create packet: {0}", Util.ByteToHex(res.GetBuffer()));

            res.Send();
        }
    }
}
