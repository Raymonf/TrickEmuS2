using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TE2Common.Models;

namespace TE2Common.Player
{
    public class Mover
    {
        public Character Character { get; set; }

        public bool Moving { get; set; }
        public Coordinate Destination { get; set; } = new Coordinate();
        public long MoveStep { get; set; } = 0;
    }
}
