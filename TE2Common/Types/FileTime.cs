using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TE2Common.Interfaces;

namespace TE2Common.Types
{
    public class FileTime : ISerializable
    {
        private readonly DateTime _time;

        public FileTime(DateTime time)
        {
            this._time = time;
        }

        public void Serialize(PacketBuffer packet)
        {
            packet.WriteUInt64(Methods.DateTimeToFileTime(_time));
        }
    }
}
