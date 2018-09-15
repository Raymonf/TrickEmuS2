using RoyT.AStar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common.Models;
using TE2Common.Player;

namespace TrickEmu2
{
    class Pathfinder
    {
        public static double Distance(Coordinate from, Coordinate to)
        {
            var distance = Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2));

            return distance;
        }

        static List<Coordinate> BresenhamLine(int x1, int y1, int x2, int y2)
        {
            var list = new List<Coordinate>();

            int w = x2 - x1;
            int h = y2 - y1;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);

            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }

            int numerator = longest >> 1;

            for (int i = 0; i <= longest; i++)
            {
                list.Add(new Coordinate()
                {
                    X = (ushort)x1,
                    Y = (ushort)y1
                });

                numerator += shortest;

                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x1 += dx1;
                    y1 += dy1;
                }
                else
                {
                    x1 += dx2;
                    y1 += dy2;
                }
            }

            return list;
        }

        static List<Coordinate> ReduceLine(List<Coordinate> oldPoints, double distance)
        {
            var res = new List<Coordinate>();

            var i = 1;
            var skipCount = 2;

            if (oldPoints.Count > 4 && oldPoints.Count <= 10)
            {
                skipCount = 4;
            }
            else if (oldPoints.Count > 10)
            {
                skipCount = 6;
            }

            if (oldPoints.Count > 2)
            {
                i = 0;
            }

            var magic = (int)(distance % (skipCount));

            foreach (var point in oldPoints)
            {
                if (i % skipCount == 0)
                {
                    res.Add(point);
                }

                i++;
            }

            // Add the last point in
            if (i < 1)
            {
                res.Add(oldPoints[oldPoints.Count - 1]);
            }

            return res;
        }

        static List<Coordinate> LastPoint(List<Coordinate> oldPoints, Coordinate from)
        {
            var list = new List<Coordinate>();

            if (oldPoints.Count > 0)
            {
                if (Distance(from, oldPoints[oldPoints.Count - 1]) <= 0.999)
                {
                    Program.logger.Debug("Distance was <= 0.999 - Reversed");
                    oldPoints.Reverse();
                }

                list.Add(oldPoints[oldPoints.Count - 1]);
            }

            return list;
        }

        static List<Coordinate> GetLinePath(Character character, Coordinate from, Coordinate to)
        {
            var diffX = from.X % 16;
            var diffY = from.Y % 16;

            var map = Data.Maps[character.Map];
            var line = BresenhamLine(from.X / 16, from.Y / 16, to.X / 16, to.Y / 16);
            //line.Reverse();
            var res = new List<Coordinate>();

            foreach (var point in line)
            {
                var added = false;

                var x = (ushort)((point.X * 16));
                var y = (ushort)((point.Y * 16));

                var col1 = (map.Collision.X * point.Y) + point.X;
                var col2 = (map.Collision.X * point.Y) + point.X + diffX;

                if (map.Collision.Data.Length > col2)
                {
                    // We can check
                    if (map.Collision.Data[col2] == 0x00)
                    {
                        added = true;

                        res.Add(new Coordinate()
                        {
                            X = (ushort)(x + diffX),
                            Y = (ushort)(y + diffY),
                        });
                    }
                }

                if (map.Collision.Data.Length > col1 && !added)
                {
                    // We can check
                    if (map.Collision.Data[col1] == 0x00)
                    {
                        added = true;

                        res.Add(new Coordinate()
                        {
                            X = x,
                            Y = y,
                        });
                    }
                }

                if (!added)
                {
                    return LastPoint(res, from);
                }
            }

            if (res.Count >= 2)
            {
                if (Distance(from, res[0]) < Distance(from, res[res.Count - 1]))
                {
                    //res.Reverse();
                }
            }

            return LastPoint(res, from);
        }

        public static List<Coordinate> GetPath(Character character, Coordinate from, Coordinate to)
        {
            var distance = Distance(from, to);

            if (distance >= 175)
            {
                // Use the line path
                return GetLinePath(character, from, to);
            }

            // Use A*
            Position[] path = new Position[0];
            
            try
            {
                path = Data.Maps[character.Map].Grid.GetPath(
                    new Position(from.X / 16, from.Y / 16),
                    new Position(to.X / 16, to.Y / 16),
                    MovementPatterns.Full);
            }
            catch
            { }

            if (path.Length < 1)
            {
                // Use the line path
                return GetLinePath(character, from, to);
            }

            var diffX = from.X % 16;
            var diffY = from.Y % 16;

            // Convert the points to Coordinate
            var coordinates = new List<Coordinate>();

            foreach (var point in path)
            {
                coordinates.Add(new Coordinate()
                {
                    X = (ushort)((point.X * 16) + diffX),
                    Y = (ushort)((point.Y * 16) + diffY)
                });
            }

            return ReduceLine(coordinates, distance);
        }
    }
}
