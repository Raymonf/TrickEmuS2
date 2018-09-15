using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TricksterMap.Data
{
    public class PointObject
    {
        public static int[] ValidTypes =
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x07, 0x09, 0x0A, 0x0D
        };

        /// <summary>
        /// Position Id / PosId
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Type of point object
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Map ID (for portals only)
        /// </summary>
        public int MapId { get; set; }

        /// <summary>
        /// X position
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y position
        /// </summary>
        public int Y { get; set; }
        
        /// <summary>
        /// Options (unconfirmed, is this always 0?)
        /// </summary>
        public int Options { get; set; } = 0;
    }
}
