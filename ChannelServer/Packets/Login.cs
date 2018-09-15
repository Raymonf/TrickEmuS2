using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class Login
    {
        public static void Handle(User user, InPacket packet)
        {
            Console.WriteLine("Login packet: ", Util.ByteToHex(packet.packet));

            string uid = packet.ReadString();
            string upw = packet.ReadString();

            Console.WriteLine("User logging in: {0} / {1}", uid, upw);

            bool loginSuccess = false;

            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT *, COUNT(*) AS count FROM users WHERE username = @userid AND password = @userpw;";
                    cmd.Parameters.AddWithValue("@userid", Methods.CleanString(uid));
                    cmd.Parameters.AddWithValue("@userpw", Methods.CleanString(upw));

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if(reader.GetInt32("count") >= 1)
                            {
                                loginSuccess = true;

                                PacketBuffer loginAck = new PacketBuffer(0x7EC, user);
                                loginAck.WriteHexString("42 72 05 51 38 20 DB 2B 27 2A D5 64 85 08 FC 04 0A C2 BF B6 EF 2F 51 60 F3 3A F4 2F D5 59 5F 58 D0 ED 8B A9 76 54 32 10");
                                loginAck.WriteBytePad(0x00, 360);
                                loginAck.WriteUInt16(40);
                                loginAck.WriteUInt16(4585);
                                loginAck.Send();

                                user.Id = reader.GetUInt32("id");
                                user.Username = reader.GetString("username");
                                user.SlotsAllowed = reader.GetInt32("char_slots");
                                user.SlotTickets = 5; // Temporary
                            }
                            else
                            {
                                PacketBuffer data = new PacketBuffer(0x2CEF, user);
                                data.WriteByteArray(new byte[] { 0x63, 0xEA, 0x00, 0x00 });
                                data.Send();
                            }
                        }
                    }
                    cmd.Dispose();
                }

                if (loginSuccess)
                {
                    CharacterList.Handle(user, packet.packet);
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
            }
        }
    }
}
