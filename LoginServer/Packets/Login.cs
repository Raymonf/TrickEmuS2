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
        public static void Handle(User user, byte[] packet)
        {
            string uid = Methods.GetString(packet, packet.Length).Substring(0, 18);
            string upw = Methods.GetString(packet, packet.Length).Substring(19, 18);

            try
            {
                using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM users WHERE username = @userid AND password = @userpw;";
                    cmd.Parameters.AddWithValue("@userid", Methods.CleanString(uid));
                    cmd.Parameters.AddWithValue("@userpw", Methods.CleanString(upw));
                    if (Convert.ToInt32(cmd.ExecuteScalar()) >= 1)
                    {
                        // Send server select
                        PacketBuffer ack = new PacketBuffer(0x2D51, user);
                        ack.WriteBytePad(0x00, 0x415);
                        ack.Send();

                        PacketBuffer servers = new PacketBuffer(0x2CEE, user);
                        servers.WriteHexString("63 77 F6 05 F5 F4 45 BA 58 A4 3C 00 01 01 01 00 01 00 44 6F 6E 20 43 61 76 61 6C 69 65 72 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 53 65 72 76 65 72 20 31 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 B8 0B 03 00 B7 0D");
                        servers.Send();
                    }
                    else
                    {
                        PacketBuffer data = new PacketBuffer(0x2CEF, user);
                        data.WriteByteArray(new byte[] { 0x63, 0xEA, 0x00, 0x00 });
                        data.Send();
                    }
                    cmd.Dispose();
                }
            }
            catch (Exception ex)
            {
                Program.logger.Error(ex, "Database error: ");
            }
        }
    }
}
