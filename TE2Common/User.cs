using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common.Models;

namespace TE2Common
{
    public class User
    {
        public UInt32 Id { get; set; }
        public string Username { get; set; }
        public string Version { get; set; }
        public Socket Socket { get; set; }
        public int NumChars { get; set; } = 0;

        // Slot expansion
        public int SlotTickets { get; set; } = 0;
        public int SlotsAllowed { get; set; } = 4;

        public SessionInfo ClientSession { get; set; }
        public SessionInfo ServerSession { get; set; }
        public ushort Sequence { get; set; } = 0;

        // GS
        public Character Character { get; set; }
    }
}
