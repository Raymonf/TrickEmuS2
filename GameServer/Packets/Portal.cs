using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common;
using TE2Common.Player;

namespace TrickEmu2.Packets
{
    public class Portal
    {
        public static void RequestMove(User user, InPacket inPacket)
        {
            var character = user.Character;
            var x = inPacket.ReadUInt16();
            var y = inPacket.ReadUInt16();

            var enterMap = -1;
            var enterX = 0;
            var enterY = 0;

            var portalX = 0;
            var portalY = 0;

            if (Data.Maps.ContainsKey(character.Map))
            {
                // Iterate through the RangeObjects
                foreach (var obj in Data.Maps[character.Map].RangeObjects)
                {
                    if (x >= obj.X1 && x <= obj.X2 && y >= obj.Y1 && y <= obj.Y2)
                    {
                        enterMap = obj.Destination;
                        
                        var destPoint = Data.Maps[obj.Destination].PointObjects.Where(po => po.MapId == character.Map).FirstOrDefault();
                        if (destPoint != null)
                        {
                            enterX = destPoint.X;
                            enterY = destPoint.Y;
                            portalX = Math.Max(obj.X1, obj.X2);
                            portalY = Math.Max(obj.Y1, obj.Y2);
                            Program.logger.Info("X: {0} / Y: {1}");
                        }

                        Program.logger.Info("{0} - Leave Zone {0} ({1}) => {2} ({3}) / X: {4}, Y: {5}", character.Name, character.Map, Data.Maps[obj.Destination].Name, obj.Destination, enterX, enterY);

                        break;
                    }
                }
            }

            CharacterMove.StartMoveB(
                user,
                new Coordinate()
                {
                    X = character.X,
                    Y = character.Y
                },
                new Coordinate()
                {
                    X = x,
                    Y = y
                },
                new Task(() =>
                {
                    if (Pathfinder.Distance(
                        new Coordinate()
                        {
                            X = character.X,
                            Y = character.Y
                        },
                        new Coordinate()
                        {
                            X = x,
                            Y = y
                        }) > 150
                    )
                    {
                        Program.logger.Info("{0} - Too far from portal to leave zone.", character.Name);
                        return;
                    }

                    if (enterMap > 0)
                    {
                        {
                            var move = new PacketBuffer(0x18, user);
                            move.WriteUInt16(character.EntityId);
                            move.WriteUInt16(character.X);
                            move.WriteUInt16(character.Y);
                            move.WriteUInt16(x);
                            move.WriteUInt16(y);
                            move.WriteByte(0x03);
                            move.Send();
                        }

                        {
                            var p1 = new PacketBuffer(0x1D8, user);
                            p1.WriteUInt16(character.EntityId);
                            p1.WriteByte(0x2E);
                            p1.Send();
                        }

                        
                        long moveStep = new Random().Next(1, 1400000000);
                        user.Character.Mover.MoveStep = moveStep;
                        
                        
                        // Change the things
                        character.Map = (ushort)enterMap;
                        character.X = (ushort)enterX;
                        character.Y = (ushort)enterY;

                        var serverInfo = new List<byte>();

                        {
                            var p2 = new PacketBuffer(0x1B, user);
                            p2.WriteUInt16(character.EntityId);

                            // D8 05 87 03
                            p2.WriteUInt16(character.X);
                            p2.WriteUInt16(character.Y);

                            p2.WriteHexString("03");
                            serverInfo.AddRange(p2.GetPacket());
                        }

                        {
                            var notifyMap = new PacketBuffer(0x99, user);
                            notifyMap.WriteUInt32(character.Map);
                            serverInfo.AddRange(notifyMap.GetPacket());
                        }

                        {
                            var p3 = new PacketBuffer(0x15, user);
                            p3.WriteUInt16(character.EntityId);
                            serverInfo.AddRange(p3.GetPacket());
                        }

                        {
                            var serverIp = new PacketBuffer(0x9A, user);
                            serverIp.WriteHexString("5D 52 37 D4");
                            serverIp.WriteUInt64(character.Id);
                            serverIp.WriteString(Program.config.Server["GameIP"]);
                            serverIp.WriteByte(0x00);
                            serverIp.WriteUInt32(uint.Parse(Program.config.Server["GamePort"]));
                            // ?
                            serverIp.WriteHexString("C3 61 00 00");
                            serverIp.WriteUInt16(character.Map);
                            serverIp.WriteUInt16(character.X);
                            serverIp.WriteUInt16(character.Y);
                            serverIp.WriteByte(0x02);

                            serverInfo.AddRange(serverIp.GetPacket());
                        }

                        user.Socket.Send(serverInfo.ToArray());
                    }
                })
            );
        }

        // 0x9C
        // 18 00 9C 00 00 00 63 00 9A
        // E2 6E F7 05 00 00 00 00 F3 5F 00 00 02
        public static void EnterMap(User user, InPacket inPacket)
        {
            var enterId = inPacket.ReadUInt();
            var foundUser = Program._users.Where(x => x.Value != null && x.Value.Character != null && x.Value.Character.Id == enterId).FirstOrDefault();
            if (foundUser.Value == null)
            {
                throw new Exception("Zone change user not found");
            }
            //Program.logger.Info("Old key [user]: {0}", Util.ByteToHex(user.ClientSession.Key));
            //Program.logger.Info("Old key [foundUser.ClientSession]: {0}", Util.ByteToHex(foundUser.Value.ClientSession.Key));

            var currSock = user.Socket;
            var currClientSession = user.ClientSession;

            //Program.logger.Info("Found user socket: {0} / New: {1}", foundUser.Value.Socket.RemoteEndPoint.ToString(), user.Socket.RemoteEndPoint.ToString());

            Program._users[user.Socket.RemoteEndPoint.ToString()] = foundUser.Value;
            user = Program._users[user.Socket.RemoteEndPoint.ToString()];
            user.Socket = currSock;
            user.ClientSession = currClientSession;
            user.ClientSession.Client = user;
            user.ServerSession = new SessionInfo
            {
                Client = user
            };

            Program._users.Remove(foundUser.Key);

            var character = user.Character;

            Program.logger.Info("{0} - Enter Zone: {1} ({2})", character.Name, Data.Maps[character.Map].Name, character.Map);
            
            character.EntityId = ++Program.EntityId;

            {
                var notifyEntityId = new PacketBuffer(0x3D1, user);
                notifyEntityId.WriteUInt32(character.EntityId);
                notifyEntityId.WriteHexString("00 00 00 00 00 00 00 00");
                notifyEntityId.Send();
            }

            var charInfo = new List<byte>();

            {
                var unk1 = new PacketBuffer(0x377, user);
                unk1.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A8 36 00 00 00 00 00 00 00 00 17 B7 51 38");
                charInfo.AddRange(unk1.GetPacket());
            }

            {
                var unk2 = new PacketBuffer(0x376, user);
                unk2.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 A8 36 00 00 00 00 00 00 00 00 17 B7 51 38");
                charInfo.AddRange(unk2.GetPacket());
            }

            {
                var unk3 = new PacketBuffer(0x3D7, user);
                unk3.WriteHexString("01 00 00 00 00 90 85 D5 41 00 00 00 00 00 00 00 00 00 00 00 00 77 86 45 5B 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                charInfo.AddRange(unk3.GetPacket());
            }

            {
                var unk4 = new PacketBuffer(0x2A4, user);
                unk4.WriteUInt16(0);
                charInfo.AddRange(unk4.GetPacket());
            }

            {
                var unk5 = new PacketBuffer(0x3E4, user);
                unk5.WriteUInt32(3);
                charInfo.AddRange(unk5.GetPacket());
            }

            {
                var unk6 = new PacketBuffer(0x3E7, user);
                unk6.WriteUInt16(character.EntityId);
                unk6.WriteHexString("00 00 00 00 00 00 00 00 00 00 00");
                charInfo.AddRange(unk6.GetPacket());
            }

            {
                var unk7 = new PacketBuffer(0x3EA, user);
                unk7.WriteUInt16(0);
                charInfo.AddRange(unk7.GetPacket());
            }

            {
                var unk8 = new PacketBuffer(0x4CC, user);
                unk8.WriteHexString("00 00 00 00");
                charInfo.AddRange(unk8.GetPacket());
            }

            {
                var unk9 = new PacketBuffer(0x1B5, user);
                unk9.WriteUInt16(character.EntityId);
                unk9.WriteUInt16(character.X);
                unk9.WriteUInt16(character.Y);
                unk9.WriteHexString("00 00 00 00 00");
                charInfo.AddRange(unk9.GetPacket());
            }

            {
                var unk10 = new PacketBuffer(0x376, user);
                unk10.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 E2 4F 4D 3E 00 00 00 00 00 00 00 00 17 B7 51 38");
                charInfo.AddRange(unk10.GetPacket());
            }

            {
                var unk11 = new PacketBuffer(0x377, user);
                unk11.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 E2 4F 4D 3E 00 00 00 00 00 00 00 00 17 B7 51 38");
                charInfo.AddRange(unk11.GetPacket());
            }

            {
                var notifyEntity = new PacketBuffer(0x09, user);
                notifyEntity.WriteUInt16(character.EntityId);
                notifyEntity.WriteUInt16(1);
                notifyEntity.WriteUInt16(character.Job);
                notifyEntity.WriteByte((byte)character.Hair);
                notifyEntity.WriteByte((byte)character.Type);
                notifyEntity.WriteUInt16(character.Level);
                notifyEntity.WriteUInt32(8); // ?
                notifyEntity.WriteUInt16(character.X);
                notifyEntity.WriteUInt16(character.Y);
                notifyEntity.WriteUInt16(character.X);
                notifyEntity.WriteUInt16(character.Y);
                notifyEntity.WriteString(character.Name);
                notifyEntity.WriteUInt32(0x00);
                notifyEntity.WriteUInt32(0x00); // ?
                notifyEntity.WriteUInt32(0x01); // ?
                notifyEntity.WriteUInt32(0); // ?
                notifyEntity.WriteUInt32(0); // ?
                notifyEntity.WriteUInt16(0); // ????
                notifyEntity.WriteByte((byte)character.Job);
                /*
                notifyEntity.WriteUInt32(63);
                notifyEntity.WriteUInt32(6903); // Item
                notifyEntity.WriteUInt32(6953); // Item
                notifyEntity.WriteUInt32(0);
                */
                notifyEntity.WriteHexString("00 00 00 00 63 00 00 00 F6 45 00 00 90 28 01 00 14 1B 00 00 46 1B 00 00 00 00 00 00 0A 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

                Program.logger.Info("Self entity packet: {0}", Util.ByteToHex(notifyEntity.GetBuffer()));

                charInfo.AddRange(notifyEntity.GetPacket());
            }

            {
                var unk12 = new PacketBuffer(0x25C, user);
                charInfo.AddRange(unk12.GetPacket());
            }

            {
                var unk13 = new PacketBuffer(0x377, user);
                unk13.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 CE 4C 3E 00 00 00 00 00 00 00 00 17 B7 51 38");
                charInfo.AddRange(unk13.GetPacket());
            }

            {
                var unk14 = new PacketBuffer(0x376, user);
                unk14.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 20 CE 4C 3E 00 00 00 00 00 00 00 00 17 B7 51 38");
                charInfo.AddRange(unk14.GetPacket());
            }

            {
                var unk15 = new PacketBuffer(0x42C, user);
                unk15.WriteHexString("00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                charInfo.AddRange(unk15.GetPacket());
            }

            {
                var unk16 = new PacketBuffer(0x9D, user);
                unk16.WriteUInt32(0);
                charInfo.AddRange(unk16.GetPacket());
            }

            {
                var healthStatus = new PacketBuffer(0x24, user);
                healthStatus.WriteHexString("29 00 00 00");
                charInfo.AddRange(healthStatus.GetPacket());
            }

            {
                var manaStatus = new PacketBuffer(0x25, user);
                manaStatus.WriteHexString("05 00 00 00");
                charInfo.AddRange(manaStatus.GetPacket());
            }

            user.Socket.Send(charInfo.ToArray());

            {
                var p1 = new PacketBuffer(0x1D8, user);
                p1.WriteUInt16(character.EntityId);
                p1.WriteByte(0x2E);
                p1.Send();
            }
        }
    }
}
