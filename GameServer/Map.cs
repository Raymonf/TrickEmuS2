//using EpPathFinding.cs;
using RoyT.AStar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TE2Common;
using TricksterMap.Data;

namespace TrickEmu2
{
    public class Map
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;

        public string NpcTableFilename { get; set; } = "";
        public List<NpcTalkEntry> NpcTalk { get; set; } = new List<NpcTalkEntry>();

        public Grid Grid { get; set; }
        public ConfigLayer Collision { get; set; }

        public List<PointObject> PointObjects { get; set; } = new List<PointObject>();
        public List<RangeObject> RangeObjects { get; set; } = new List<RangeObject>();

        public void SendEntities(User user)
        {
            foreach (var npc in NpcTalk)
            {
                var buffer = new PacketBuffer(0x8, user);
                buffer.WriteUInt16(npc.EntityId);
                buffer.WriteUInt16(2); // Type? 2 = NPC
                buffer.WriteUInt32((uint)npc.Type);
                buffer.WriteUInt32(0); // ?
                buffer.WriteUInt16(0); // ?

                var pos = PointObjects.FirstOrDefault(x => x.Id == npc.PosId);

                if (pos != null)
                {
                    buffer.WriteUInt16((ushort)pos.X);
                    buffer.WriteUInt16((ushort)pos.Y);
                    buffer.WriteUInt16((ushort)pos.X);
                    buffer.WriteUInt16((ushort)pos.Y);
                }
                else
                {
                    Program.logger.Error("------ Entity position {0} not found for NPC {1} in map {2} ({3})", npc.PosId, npc.Type, Name, Id);

                    buffer.WriteUInt16(0);
                    buffer.WriteUInt16(0);
                    buffer.WriteUInt16(0);
                    buffer.WriteUInt16(0);
                }

                buffer.WriteString(Methods.Utf8Convert(Data.CharacterInfo[npc.Type].Name));
                buffer.WriteByte(0x00);
                
                buffer.WriteHexString("00 80 00 00 00 00 00 00 00 00 00 00"); // ?
                buffer.WriteUInt16((ushort)npc.PosId);
                buffer.WriteUInt16((ushort)npc.HeadMarkType); // Head marking
                buffer.WriteUInt16(0); // ?

                Program.logger.Debug("Sending entity packet: {0}", Util.ByteToHex(buffer.GetBuffer()));

                buffer.Send();
            }
        }
    }
}
