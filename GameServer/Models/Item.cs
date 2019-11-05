using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrickEmu2.Models
{
    public enum ItemClass
    {
        Common = 1,
        Rare = 2,
        MyCamp = 3 // ?
    }

    public class Item
    {
        public ItemClass Class { get; set; }
        public uint Id { get; set; }
        public ulong Uid { get; set; }
        public ushort Count { get; set; } = 1;

        public byte Unknown1 { get; set; } = 0;
        public byte RefineLevel { get; set; } = 0;
        public byte RefineState { get; set; } = 0;
        public byte ItemSlot { get; set; } = 0;
        public byte Unknown2 { get; set; } = 0;
    }
}
