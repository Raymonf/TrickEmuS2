using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common;
using TE2Common.Interfaces;

namespace TrickEmu2.Models
{
    public enum ItemClass
    {
        Common = 1,
        Rare = 2,
        MyCamp = 3 // ?
    }

    public enum AttributeIndex : uint
    {
        AP = 10000, // Stats
        AC,
        DX,
        MP,
        MA,
        MD,
        WT,
        DA,
        LK,
        HP,
        DP,
        HV,
        GunAP,
        FireAdd = 20000, // Elemental
        WaterAdd,
        WindAdd,
        EarthAdd,
        ElectricAdd,
        LightAdd,
        DarkAdd,
        NoAdd, // neutral attr?
        ShadowAdd,
        FireResist = 21000,
        WaterResist,
        WindResist,
        SoilResist,
        ResistanceResist,
        LightResist,
        DarkResist,
        NoResist, // neutral resist?
        PhysicalResist,
        UnknownResist, // 21009 - ????, probably used by some monsters
    }

    public class ItemAttribute : ISerializable
    {
        // not sure what 10 is, but 0xA is very common
        public uint Type { get; set; } = 10;

        // type of option (10000 ...)
        public AttributeIndex Index { get; set; } = AttributeIndex.AP;

        // is this the right type? or is it supposed to be a int in the packet?
        // if int then MixedItem needs to become ushort
        public short Value { get; set; } = 0;

        // type? see above
        public uint MixedItem { get; set; } = 0;

        public void Serialize(PacketBuffer packet)
        {
            packet.WriteUInt32(Type);
            packet.WriteUInt32((uint)Index);
            packet.WriteInt16(Value);
            packet.WriteUInt32(MixedItem);
        }
    }

    public class Item : IItem
    {
        public ItemClass Class { get; set; }
        public uint Id { get; set; }
        public ulong Uid { get; set; }
        public ushort Count { get; set; } = 1;
        public byte Unknown1 { get; set; } = 0;
        public byte RefineLevel { get; set; } = 0;
        public byte RefineState { get; set; } = 0;
        public byte ItemSlot { get; set; } = 0;
        public List<ItemAttribute> Attributes { get; set; } = new List<ItemAttribute>();

        public void Serialize(PacketBuffer packet)
        {
            packet.WriteByte((byte)Class);
            packet.WriteUInt32(Id); // Item ID
            packet.WriteUInt64(Uid); // Item UID
            packet.WriteUInt16(Count); // Count
            if (Class == ItemClass.Rare)
            {
                packet.WriteByte(0); // Unknown
                packet.WriteByte(RefineLevel); // Refine Level
                packet.WriteByte(RefineState); // Refine State
                packet.WriteByte(ItemSlot); // Item Slot
                packet.WriteByte((byte)Attributes.Count);
                foreach (var attr in Attributes)
                {
                    attr.Serialize(packet);
                }
            }
        }
    }
}
