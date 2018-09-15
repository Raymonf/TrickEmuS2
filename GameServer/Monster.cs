using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrickEmu2
{
    public class Monster
    {
        public ushort EntityId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public Task Task { get; set; }
    }
}
