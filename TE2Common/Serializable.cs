using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TE2Common
{
    public interface ISerializable
    {
        void Serialize(PacketBuffer packet);
    }
}
