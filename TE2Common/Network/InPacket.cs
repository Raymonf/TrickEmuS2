using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TE2Common
{
    public class InPacket
    {
        public byte[] packet;
        int pos = 0;

        public InPacket(byte[] p)
        {
            packet = p;
            pos = 0;
        }

        public void Seek(long offset)
        {
            pos = (int)offset;
        }

        public int Read()
        {
            return packet[pos++] & 0xFF;
        }

        public byte ReadByte()
        {
            return (byte)Read();
        }

        public ushort ReadUInt16()
        {
            var i = BitConverter.ToUInt16(packet, pos);

            Seek(pos + 2);

            return i;
        }

        public int ReadInt()
        {
            var i = BitConverter.ToInt32(packet, pos);

            Seek(pos + 4);

            return i;
        }

        public uint ReadUInt()
        {
            var i = BitConverter.ToUInt32(packet, pos);

            Seek(pos + 4);

            return i;
        }

        public long ReadLong()
        {
            var i = BitConverter.ToInt64(packet, pos);

            Seek(pos + 8);

            return i;
        }

        public string ReadString()
        {
            var b = Methods.ReadTerminatedStringToBytes(packet, pos);
            
            pos += b.Length + 1;

            return Constants.Encoding.GetString(b);
        }
    }
}
