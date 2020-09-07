using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;
using TE2Common.Models;
using TrickEmu2.Packets.Login;

namespace TrickEmu2.Packets
{
    class OnLogin
    {
        public static void Handle(User user, InPacket packet)
        {
            //Console.WriteLine("Login packet: ", Util.ByteToHex(packet.packet));

            var charId = packet.ReadUInt();
            packet.ReadUInt();
            var loginKey = packet.ReadUInt();

            // TODO: Check the loginKey
            // The login key is sent to the client by 0x7DE (from Channel)

            //Console.WriteLine("Character logging in: {0}", charId);

            using (MySqlCommand cmd = Program._MySQLConn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM characters WHERE id = @id AND authority != 2 LIMIT 1;";
                cmd.Parameters.AddWithValue("@id", charId);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var build = reader.GetString("build").Split(',');
                        byte slot = reader.GetByte("slot");
                        byte job2 = reader.GetByte("job2");
                        byte type2 = reader.GetByte("job2_type");
                        byte job3 = reader.GetByte("job3");
                        byte type3 = reader.GetByte("job3_type");
                        user.Character = new Character()
                        {
                            User = user,
                            Slot = slot,
                            Id = reader.GetUInt64("id"),
                            Name = reader.GetString("name"),
                            Level = reader.GetUInt16("level"),
                            Money = reader.GetUInt32("money"),
                            Health = reader.GetUInt16("health"),
                            Mana = reader.GetUInt16("mana"),
                            Map = reader.GetUInt16("map"),
                            X = reader.GetUInt16("pos_x"),
                            Y = reader.GetUInt16("pos_y"),
                            FType = reader.GetByte("ftype"),
                            Job = reader.GetByte("job"),
                            Type = reader.GetByte("type"),
                            Hair = reader.GetByte("hair"),
                            Build = new CharacterBuild
                            {
                                Power = int.Parse(build[0]),
                                Magic = int.Parse(build[1]),
                                Sense = int.Parse(build[2]),
                                Charm = int.Parse(build[3])
                            },
                            EntityId = ++Program.EntityId,
                            CreateTime = reader.GetDateTime("create_time"),
                            Job2 = job2,
                            Type2 = type2,
                            Job3 = job3,
                            Type3 = type3,
                        };
                    }
                }
            }

            var character = user.Character;

            Program.logger.Info("{0} is logging in. Map: {1} ({2})", character.Name, Data.Maps[character.Map].Name, character.Map);

            var success = new PacketBuffer(0x2A7, user);
            success.WriteUInt64(charId);
            success.WriteUInt64(user.Id);

            //success.WriteHexString("01 64 75 72 61 67 6F 6E 31 32 34 00 00 00 68 00 00 00 00 00 00 04 00 04 01 01 04 03 02 5C 51 42 5B 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            
            success.WriteByte(character.Slot);
            success.WriteString(character.Name, 20);

            success.WriteUInt16(character.Job); // type? is it {byte} 0x00?
            success.WriteByte(character.Type);
            success.WriteByte(character.FType); // hair color? ftype?

            success.WriteByte((byte)character.Build.Power);
            success.WriteByte((byte)character.Build.Magic);
            success.WriteByte((byte)character.Build.Sense);
            success.WriteByte((byte)character.Build.Charm);

            // ?
            success.WriteUInt64(Methods.DateTimeToUnix(character.CreateTime));
            // ?
            success.WriteUInt16(character.Type2);
            success.WriteByte(character.Job2);
            success.WriteUInt16(character.Type3);
            success.WriteByte(character.Job3);
            success.WriteBool(character.IsSoulSeed);
            success.WriteBool(character.IsAwaken);
            if (character.Couple != null)
            {
                success.WriteUInt64(character.Couple.Id);
            }
            else
            {
                success.WriteUInt64(0); // nonexistent
            }
            success.WriteHexString("00 00 00 00"); // is this used?
            
            success.Send();


            var charInfo = new List<byte>();

            var head = new PacketBuffer(0xE6, user);
            head.WriteHexString("0D D7 76 CE");
            head.WriteUInt16(character.EntityId);
            head.WriteUInt16(character.X);
            head.WriteUInt16(character.Y);
            charInfo.AddRange(head.GetPacket());

            var items = new PacketBuffer(0xE7, user);
            items.WriteUInt16(8); // Item count

            
            items.WriteByte(0x02); // Item class 2
            items.WriteUInt32(6903); // Item ID
            items.WriteUInt64(3031780617); // Item UID
            items.WriteUInt16(1); // Count
            // Refine? Time?
            items.WriteUInt32(0); // ?
            items.WriteByte(0x00); // ?


            items.WriteByte(0x02); // Item class 2
            items.WriteUInt32(6953); // Item ID
            items.WriteUInt64(3031780618); // Item UID
            items.WriteUInt16(1); // Count
            // Refine? Time?
            items.WriteUInt32(0); // ?
            items.WriteByte(0x00); // ?


            items.WriteByte(0x01); // Item class 1
            items.WriteUInt32(2006); // Item ID
            items.WriteUInt64(3031780619); // Item UID
            items.WriteUInt16(30); // Count
            // Item class doesn't need refine/time/whatever


            items.WriteByte(0x01); // Item class 1
            items.WriteUInt32(2206); // Item ID
            items.WriteUInt64(3031780620); // Item UID
            items.WriteUInt16(30); // Count
            // Item class doesn't need refine/time/whatever


            items.WriteByte(0x02); // Item class 2
            items.WriteUInt32(17310); // Item ID
            items.WriteUInt64(3031780621); // Item UID
            items.WriteUInt16(1); // Count
            items.WriteByte(0); // Unknown?
            items.WriteByte(0); // Refine Level
            items.WriteByte(0); // Refine State
            items.WriteByte(0); // Item Slot
            items.WriteByte(0); // Unknown? Num attrs?
            // 0x14 = 20 // 0A 00 00 00 10 27 FE 7F 00 00 00 00
            //items.WriteUInt32(0); // ?
            //items.WriteByte(0x00); // ?


            items.WriteByte(0x02); // Item class 2
            items.WriteUInt32(17910); // Item ID
            items.WriteUInt64(3031780622); // Item UID
            items.WriteUInt16(1); // Count
            items.WriteUInt32(0); // ?
            items.WriteByte(0x00); // ?


            items.WriteByte(0x02); // Item class 2
            items.WriteUInt32(18810); // Item ID
            items.WriteUInt64(3031780623); // Item UID
            items.WriteUInt16(1); // Count
            items.WriteUInt32(0); // ?
            items.WriteByte(0x00); // ?


            items.WriteByte(0x02); // Item class 2
            items.WriteUInt32(19241); // Item ID
            items.WriteUInt64(3031780624); // Item UID
            items.WriteUInt16(1); // Count
            items.WriteUInt32(0); // ?
            items.WriteByte(0x00); // ?

            charInfo.AddRange(items.GetPacket());


            var stats = new PacketBuffer(0xE9, user);

            //stats.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 09 4D B5 B4 00 00 00 00 0A 4D B5 B4 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 01 00 00 01 00 94 00 00 00 00 00 00 00 00 00 94 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 09 00 01 50 00 00 00");
            
            // ?
            stats.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

            // Item UID of Dragon`s ears
            stats.WriteUInt64(3031780617);

            // Item UID of Dragon`s tail
            stats.WriteUInt64(3031780618);

            stats.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            stats.WriteHexString("01 00 00 01 00");
            stats.WriteUInt16(character.Map);
            stats.WriteHexString("00 00 00 00 00 00 00 00");
            stats.WriteUInt16(character.Map);
            stats.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            
            // ?
            stats.WriteUInt16(character.Hair);

            // ?
            stats.WriteByte(0x1);

            // ?
            stats.WriteUInt32(0x50);

            charInfo.AddRange(stats.GetPacket());

            
            // MonsterQuest
            var monQuest = new PacketBuffer(0xEB, user);
            monQuest.WriteUInt16(0);
            charInfo.AddRange(monQuest.GetPacket());
            
            // SkillQuest
            var skillQuest = new PacketBuffer(0x128, user);
            skillQuest.WriteUInt16(0);
            charInfo.AddRange(skillQuest.GetPacket());
            
            // Skill
            var skill = new PacketBuffer(0x10F, user);
            skill.WriteUInt16(1); // Skill count
            // Magical Soul
            skill.WriteUInt16(2014); // Skill ID
            skill.WriteByte(0x01); // Skill ID
            skill.WriteByte(0x0D); // ?
            skill.WriteByte(0xB4); // ?
            charInfo.AddRange(skill.GetPacket());

            var itemExpiry = new PacketBuffer(0x1FC, user);
            itemExpiry.Put(new ItemExpiry(character));
            charInfo.AddRange(itemExpiry.GetPacket());

            var unknown2 = new PacketBuffer(0x341, user);
            unknown2.WriteUInt16(0);
            charInfo.AddRange(unknown2.GetPacket());
            
            var unknown3 = new PacketBuffer(0x4ED, user);
            unknown3.WriteUInt16(0);
            charInfo.AddRange(unknown3.GetPacket());

            var unknown4 = new PacketBuffer(0x05, user);
            unknown4.WriteByte(0x00);
            charInfo.AddRange(unknown4.GetPacket());

            var unknown5 = new PacketBuffer(0x04EF, user);
            unknown5.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            charInfo.AddRange(unknown5.GetPacket());

            var notifyEntityId = new PacketBuffer(0x3D1, user);
            notifyEntityId.WriteUInt16(character.EntityId);
            notifyEntityId.WriteHexString("00 00 00 00 00 00 00 00 00 00");
            charInfo.AddRange(notifyEntityId.GetPacket());

            var unknown7 = new PacketBuffer(0x377, user);
            unknown7.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A8 36 00 00 00 00 00 00 00 00 17 B7 51 38 00 00 00 00 00 00 00 00");
            charInfo.AddRange(unknown7.GetPacket());

            var unknown8 = new PacketBuffer(0x376, user);
            unknown8.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A8 36 00 00 00 00 00 00 00 00 17 B7 51 38 00 00 00 00 00 00 00 00");
            charInfo.AddRange(unknown8.GetPacket());

            var unknown9 = new PacketBuffer(0x3D7, user);
            unknown9.WriteHexString("01 00 00 00 00 90 85 D5 41 00 00 00 00 00 00 00 00 00 00 00 00 DC 54 42 5B 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            charInfo.AddRange(unknown9.GetPacket());

            var unknown10 = new PacketBuffer(0x2A4, user);
            unknown10.WriteUInt16(0);
            charInfo.AddRange(unknown10.GetPacket());

            var unknown11 = new PacketBuffer(0x3E4, user);
            unknown11.WriteUInt32(3);
            charInfo.AddRange(unknown11.GetPacket());

            var unknown12 = new PacketBuffer(0x3E7, user);
            unknown12.WriteUInt16(character.EntityId);
            unknown12.WriteHexString("00 00 00 00 00 00 00 00 00 00 00");
            charInfo.AddRange(unknown12.GetPacket());

            var unknown13 = new PacketBuffer(0x3EA, user);
            unknown13.WriteUInt16(0);
            charInfo.AddRange(unknown13.GetPacket());

            user.Socket.Send(charInfo.ToArray());
        }

        // 0x05
        public static void HandleSelect2(User user, byte[] packet)
        {
            var p1 = new PacketBuffer(0x2E29, user);
            p1.WriteHexString("00 00 00");
            p1.Send();

            var full = new List<byte>();

            var player = new PacketBuffer(0x07, user);
            player.WriteUInt16(user.Character.EntityId);

            // wrong type?
            //player.WriteHexString("01 00 04 00 09 04 01 00 08 00 00 00 B5 03 B4 08 B5 03 B4 08 64 75 72 61 67 6F 6E 31 32 34 00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 04 00 00 00 00 60 00 00 00 F7 1A 00 00 29 1B 00 00 00 00 00 00 09 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
            
            player.WriteUInt16(1); // ? (user.Character.FType);
            player.WriteUInt16(user.Character.Type);
            player.WriteByte((byte)user.Character.Hair);
            player.WriteByte((byte)user.Character.Type);
            player.WriteUInt16(user.Character.Level); // ?
            player.WriteUInt32(8); // ?
            player.WriteUInt16(user.Character.X);
            player.WriteUInt16(user.Character.Y);
            player.WriteUInt16(user.Character.X);
            player.WriteUInt16(user.Character.Y);
            player.WriteString(user.Character.Name);

            player.WriteHexString("00 00 00 00 00 00 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 04 00 00 00 00 60 00 00 00");

            // equip
            player.WriteUInt32(0x1AF7);
            // equip
            player.WriteUInt32(0x1B29);

            player.WriteHexString("00 00 00 00 00 00 09 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

            Program.logger.Info("GS Login player packet: {0}", Util.ByteToHex(player.GetBuffer()));

            full.AddRange(player.GetPacket());

            var unk1 = new PacketBuffer(0x135, user);
            full.AddRange(unk1.GetPacket());

            var unk2 = new PacketBuffer(0x25C, user);
            full.AddRange(unk2.GetPacket());
            
            var unk5 = new PacketBuffer(0x3A6, user);
            unk5.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00");
            full.AddRange(unk5.GetPacket());

            var unk6 = new PacketBuffer(0x3A5, user);
            unk6.WriteUInt16(user.Character.EntityId);
            unk6.WriteHexString("00 00");
            full.AddRange(unk6.GetPacket());

            var unk7 = new PacketBuffer(0x325, user);
            unk7.WriteHexString("AD E6 76 CE");
            full.AddRange(unk7.GetPacket());

            var time = new PacketBuffer(0x477, user);
            time.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00");
            time.WriteUInt64(Methods.DateTimeToUnix(DateTime.UtcNow));
            full.AddRange(time.GetPacket());

            var unk9 = new PacketBuffer(0x4CC, user);
            unk9.WriteHexString("00 00 00 00");
            full.AddRange(unk9.GetPacket());

            var healthStatus = new PacketBuffer(0x24, user);
            healthStatus.WriteHexString("29 00 00 00");
            full.AddRange(healthStatus.GetPacket());

            var manaStatus = new PacketBuffer(0x25, user);
            manaStatus.WriteHexString("05 00 00 00");
            full.AddRange(manaStatus.GetPacket());

            user.Socket.Send(full.ToArray());

            Data.Maps[user.Character.Map].SendEntities(user);
        }
    }
}
