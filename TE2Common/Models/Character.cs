using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common.Interfaces;
using TE2Common.Player;

namespace TE2Common.Models
{
    public class Character
    {
        public User User { get; set; }

        public byte Slot { get; set; } = 0;
        public ulong Id { get; set; }
        public string Name { get; set; }

        public ushort Level { get; set; } = 1;

        public uint Money { get; set; }

        public uint Health { get; set; } = 0;
        public uint Mana { get; set; } = 0;

        public ushort Map { get; set; } = 33;
        public ushort X { get; set; } = 768;
        public ushort Y { get; set; } = 768;

        public byte FType { get; set; }
        public byte Job { get; set; }
        public byte Type { get; set; }
        public ushort Type2 { get; set; } = 0;
        public byte Job2 { get; set; } = 0;
        public ushort Type3 { get; set; } = 0;
        public byte Job3 { get; set; } = 0;

        public ushort Hair { get; set; }

        public CharacterBuild Build { get; set; }

        public ushort EntityId { get; set; } = 0;

        public double MoveSpeed_Walk { get; set; } = 1.0;
        public double MoveSpeed_Run { get; set; } = 3.0;

        public bool IsSoulSeed { get; set; } = false;
        public bool IsAwaken { get; set; } = false;
        public Character Couple { get; set; } = null;

        public Mover Mover { get; set; } = new Mover();

        public List<IItem> Items { get; set; } = new List<IItem>();
        public DateTime CreateTime { get; set; }
    }
}
