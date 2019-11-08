using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common;
using TE2Common.Interfaces;
using TE2Common.Models;
using TE2Common.Types;
using TrickEmu2.Models;

namespace TrickEmu2.Packets.Login
{
    class ItemExpiry : ISerializable
    {
        private readonly Character character;
        private readonly List<Item> timerItems = new List<Item>();

        public ItemExpiry(Character character)
        {
            this.character = character;
            
            foreach (var i in character.Items)
            {
                var item = (Item)i;

                if (item.ExistType == ExistType.Timed)
                {
                    timerItems.Add(item);
                }
            }
        }

        public void Serialize(PacketBuffer packet)
        {
            packet.Put(
                new FileTime(DateTime.UtcNow)
            );
            packet.WriteUInt16((ushort)timerItems.Count);
            foreach (var item in timerItems)
            {
                packet.WriteUInt64(item.Uid);
                packet.WriteUInt32(item.Id);
                packet.Put(new FileTime(item.ExpiryTime));
                packet.WriteBool(DateTime.Now > item.ExpiryTime);
            }
        }
    }
}
