using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TrickEmu2.Packets
{
    class Disconnect
    {
        public static void Handle(User user, byte[] packet)
        {
            Program._users.Remove(user.Socket.RemoteEndPoint.ToString());
            Program._clientSockets.Remove(user.Socket);
            Program._users.Remove(user.Socket.RemoteEndPoint.ToString());
            Program.logger.Warn("Client {0} has disconnected.", user.Socket.RemoteEndPoint.ToString());
            try
            {
                user.Socket.Close();
            }
            catch { }
        }
    }
}
