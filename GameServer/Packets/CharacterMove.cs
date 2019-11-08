//using EpPathFinding.cs;
using RoyT.AStar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TE2Common;
using TE2Common.Player;

namespace TrickEmu2.Packets
{
    class CharacterMove
    {
        public static void StartMoveB(User user, Coordinate toPos, Coordinate fromPos, Task endTask = null)
        {
            Program.logger.Info("-------------- STARTING MOVE");

            var character = user.Character;
            
            var path = Pathfinder.GetPath(character, fromPos, toPos);

            character.X = fromPos.X;
            character.Y = fromPos.Y;
            character.Mover.Destination = toPos;

            long moveStep = new Random().Next(1, 1400000000);
            user.Character.Mover.MoveStep = moveStep;

            new Task(async () =>
            {
                Program.logger.Info("inside mover task! #points = {0}", path.Count);
                
                foreach (var point in path)
                {
                    Program.logger.Info("moving to {0} / {1}", point.X, point.Y);

                    user.Character.Mover.Moving = true;

                    if (user.Character.Mover.MoveStep != moveStep)
                    {
                        Program.logger.Warn("MoveStep changed, returning");
                        return;
                    }

                    var dist = Pathfinder.Distance(
                        new Coordinate()
                        {
                            X = user.Character.X,
                            Y = user.Character.Y
                        },
                        point
                    );

                    if ((int)dist == 0)
                    {
                        Program.logger.Debug("Distance was 0, ignoring");
                        continue;
                    }

                    var res = new PacketBuffer(0x18, user);
                    res.WriteUInt16(user.Character.EntityId);
                    //res.WriteUInt16(user.Character.X);
                    //res.WriteUInt16(user.Character.Y);
                    res.WriteUInt16(point.X);
                    res.WriteUInt16(point.Y);
                    res.WriteUInt16(point.X);
                    res.WriteUInt16(point.Y);
                    res.WriteByte(0x2);
                    res.Send();

                    Program.logger.Info("Distance: {0}", dist);

                    if (dist > 0 && user.Character.Mover.MoveStep == moveStep)
                    {
                        var waitTime = (int)((5 * dist) + 10 - (-10 * user.Character.MoveSpeed_Run));

                        user.Character.X = point.X;
                        user.Character.Y = point.Y;

                        await Task.Delay(waitTime);

                        Program.logger.Debug("waited thread");
                    }
                }

                if (user.Character.Mover.MoveStep == moveStep && endTask != null)
                {
                    await Task.Delay((int)(300 - (-10 * user.Character.MoveSpeed_Run)));

                    endTask.Start();
                }
            }).Start();

            //character.Mover.Task.Start();
        }

        static void StartMove()
        {

        }

        public static void Handle(User user, byte[] packet)
        {
            var x = BitConverter.ToUInt16(packet, 4);
            var y = BitConverter.ToUInt16(packet, 6);
            Program.logger.Info($"{BitConverter.ToUInt16(packet, 0)} {BitConverter.ToUInt16(packet, 2)} {BitConverter.ToUInt16(packet, 4)} {BitConverter.ToUInt16(packet, 6)}");
            Program.logger.Info("Start Move - FROM: X: {0}  Y: {1} / TO: X: {2}  Y: {3}", user.Character.X, user.Character.Y, x, y);

            var coord = new Coordinate()
            {
                X = x,
                Y = y
            };

            StartMoveB(user, coord, new Coordinate() { X = BitConverter.ToUInt16(packet, 0), Y = BitConverter.ToUInt16(packet, 2) });

            /*

            */

        }

        public static void EndMove(User user, int numMoves)
        {
            //user.Character.Mover.Task.Dispose();
            user.Character.Mover.Moving = false;

            if (numMoves > 0)
            {
                var res = new PacketBuffer(0x18, user);
                res.WriteUInt16(user.Character.EntityId);
                res.WriteUInt16(user.Character.X);
                res.WriteUInt16(user.Character.Y);
                res.WriteUInt16(user.Character.Mover.Destination.X);
                res.WriteUInt16(user.Character.Mover.Destination.Y);
                res.WriteByte(0x2);
                res.Send();

                user.Character.X = user.Character.Mover.Destination.X;
                user.Character.Y = user.Character.Mover.Destination.Y;
            }
        }
    }
}
