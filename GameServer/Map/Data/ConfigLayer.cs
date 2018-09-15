using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TricksterMap.Data
{
    public class ConfigLayer
    {
        /// <summary>
        /// The layer's type
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// The layer's X size
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The layer's Y size
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// The layer's X BPP (bits per pixel)
        /// Probably 0x10/16bpp
        /// </summary>
        public int BppX { get; set; }

        /// <summary>
        /// The layer's Y BPP (bits per pixel)
        /// Probably 0x10/16bpp
        /// </summary>
        public int BppY { get; set; }

        /// <summary>
        /// Actual layer data
        /// </summary>
        public byte[] Data { get; set; }
    }
}
