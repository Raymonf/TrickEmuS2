using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common.Player;

namespace TE2Common.Models
{
    public class Character
    {
        public User User { get; set; }

        public ulong Id { get; set; }
        public string Name { get; set; }

        public ushort Level { get; set; } = 1;

        public uint Money { get; set; }

        public uint Health { get; set; } = 0;
        public uint Mana { get; set; } = 0;

        public ushort Map { get; set; } = 33;
        public ushort X { get; set; } = 768;
        public ushort Y { get; set; } = 768;

        public ushort FType { get; set; }
        public ushort Job { get; set; }
        public ushort Type { get; set; }

        public ushort Hair { get; set; }

        public Build Build { get; set; }

        public ushort EntityId { get; set; } = 0;

        public double MoveSpeed { get; set; } = 1.0;

        public Mover Mover { get; set; } = new Mover();
    }
}
