using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TE2Common;

namespace TE2Common
{
    public class PacketBuffer
    {
        private List<byte> packet;
        private readonly ushort opcode;
        private User user;

        public PacketBuffer(ushort opcode, User user)
        {
            // initial capacity of 512
            packet = new List<byte>(512);
            this.opcode = opcode;
            this.user = user;
        }

        /// <summary>
        /// Returns the packet payload as a byte array
        /// </summary>
        public byte[] GetPacket()
        {
            // Encrypt the packet
            lock (user.ServerSession)
            {
                var packer = new Packer();
                var data = packet.ToArray();

                return packer.Pack(user.ServerSession, opcode, data);
            }
        }

        /// <summary>
        /// Gets the buffer without encryption
        /// </summary>
        public byte[] GetBuffer()
        {
            return packet.ToArray();
        }
        
        public void Send()
        {
            var pkt = GetPacket();

            Console.WriteLine("[Out] " + Util.ByteToHex(pkt));

            try
            {
                lock (user.ServerSession)
                {
                    user.Socket.Send(pkt);
                }
            }
            catch { }
            finally
            {
                //user.ServerSession.Mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Writes a byte to the packet "buffer"
        /// </summary>
        /// <param name="b">The byte to write</param>
        public void WriteByte(byte b)
        {
            packet.Add(b);
            return;
        }

        /// <summary>
        /// Writes a byte array to the packet "buffer"
        /// </summary>
        /// <param name="b">The byte array to write</param>
        public void WriteByteArray(byte[] b)
        {
            packet.AddRange(b);
            return;
        }

        /// <summary>
        /// Writes a UInt32 to the packet "buffer"
        /// </summary>
        /// <param name="i">The Uint32 to write</param>
        public void WriteUInt32(uint i, bool flip = false)
        {
            if (flip)
            {
                packet.AddRange(BitConverter.GetBytes(i).Reverse().ToArray());
                return;
            }

            packet.AddRange(BitConverter.GetBytes(i));
            return;
        }

        /// <summary>
        /// Writes a Int16 to the packet "buffer"
        /// </summary>
        /// <param name="i">The int to write</param>
        public void WriteInt16(short i)
        {
            packet.AddRange(BitConverter.GetBytes(i));
            return;
        }

        /// <summary>
        /// Writes a Int32 to the packet "buffer"
        /// </summary>
        /// <param name="i">The int to write</param>
        public void WriteInt32(int i)
        {
            packet.AddRange(BitConverter.GetBytes(i));
            return;
        }

        /// <summary>
        /// Writes a Int64 to the packet "buffer"
        /// </summary>
        /// <param name="i">The int64 to write</param>
        public void WriteInt64(long i)
        {
            packet.AddRange(BitConverter.GetBytes(i));
            return;
        }

        /// <summary>
        /// Writes a byte n times to the packet "buffer"
        /// </summary>
        /// <param name="b">The byte to write n times</param>
        /// <param name="n">The amount of times to repeat-write b</param>
        public void WriteBytePad(byte b, int n)
        {
            List<byte> _temp = new List<byte>();
            for (int i = 1; i <= n; i++)
            {
                _temp.Add(b);
            }
            packet.AddRange(_temp.ToArray());
            return;
        }

        /// <summary>
        /// Writes a UInt16 (UInt16) to the packet "buffer"
        /// </summary>
        /// <param name="us">The UInt16 to write</param>
        public void WriteUInt16(ushort us, bool flip = false)
        {
            if(flip)
            {
                packet.AddRange(BitConverter.GetBytes(us).Reverse().ToArray());
                return;
            }

            packet.AddRange(BitConverter.GetBytes(us));
            return;
        }

        /// <summary>
        /// Writes a UInt64 to the packet "buffer"
        /// </summary>
        /// <param name="i">The UInt64 to write</param>
        public void WriteUInt64(ulong i)
        {
            packet.AddRange(BitConverter.GetBytes(i));
            return;
        }

        /// <summary>
        /// Writes a hex string to the packet "buffer"
        /// </summary>
        /// <param name="str">The hex string to write</param>
        public void WriteHexString(string str)
        {
            packet.AddRange(StringToByteArray(str.Replace(" ", "")));
            return;
        }

        /// <summary>
        /// Writes a string to the packet "buffer"
        /// </summary>
        /// <param name="str">The string to write</param>
        public void WriteString(string str)
        {
            packet.AddRange(Constants.Encoding.GetBytes(str));
            return;
        }

        /// <summary>
        /// Writes a string to the packet "buffer" with a fixed length
        /// If the string is smaller than the length, 0x00s will be written
        /// </summary>
        /// <param name="str">The string to write</param>
        /// <param name="str">The length of the string</param>
        public void WriteString(string str, int len)
        {
            var b = Constants.Encoding.GetBytes(str);
            
            if (b.Length > len)
            {
                WriteByteArray(b);
            }
            else
            {
                WriteByteArray(b.Take(len).ToArray());
            }

            if (b.Length < len)
            {
                for (int i = b.Length; i < len; i++) WriteByte(0x00);
            }
        }

        /// <summary>
        /// Converts a hex string to a byte array
        /// http://stackoverflow.com/a/321404/1908515
        /// Author: JaredPar http://stackoverflow.com/users/23283/jaredpar (and editors)
        /// </summary>
        public byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
