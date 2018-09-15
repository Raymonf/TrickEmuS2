using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TE2Common
{
    public class Packer
    {
        ushort originalOpcode = 0;
        byte[] originalData = null;
        

        void PackHeader(SessionInfo sessionInfo, byte[] data, byte randKey)
        {
            // Update the randKey
            data[6] = randKey;

            // Update the checkflag
            data[8] = Common.MakeChecksum(data, sessionInfo.Key);

            // Calculate length
            byte bl = sessionInfo.Key;
            int ecx = randKey;
            int edx = data[7]; // packing
            ecx <<= 8;
            ecx += edx;
            byte al = KeyTable.Table[(ushort)ecx];
            data[7] = al; // update packing
            edx = data[7];
            ecx = data[6];
            ecx ^= edx;
            edx = data[1]; // wtf
            ecx <<= 8;
            ecx += edx;
            int edi = bl;
            byte bh = KeyTable.Table[(ushort)ecx];
            ecx = data[7];
            edx = data[0];
            ecx ^= edi;
            ecx <<= 8;
            ecx += edx;
            bl = KeyTable.Table[(ushort)ecx];
            ushort bx = (ushort)((bh << 8) + bl);
            Util.CopyTo(data, 0, bx);

            // Calculate opcode
            ecx = data[1];
            edx = data[3];
            ecx ^= edi;
            ecx <<= 8;
            ecx += edx;
            bh = KeyTable.Table[(ushort)ecx];
            edx = data[0];
            ecx = data[6];
            ecx ^= edx;
            edx = data[2];
            ecx <<= 8;
            ecx += edx;
            bl = KeyTable.Table[(ushort)ecx];
            bx = (ushort)((bh << 8) + bl);
            Util.CopyTo(data, 2, bx);

            // Calculate checkflag
            edx = data[7];
            ecx = data[3];
            ecx ^= edx;
            edx = data[5];
            ecx <<= 8;
            ecx += edx;
            bh = KeyTable.Table[(ushort)ecx];
            ecx = data[2];
            edx = data[4];
            ecx ^= edi;
            ecx <<= 8;
            ecx += edx;
            bl = KeyTable.Table[(ushort)ecx];
            bx = (ushort)((bh << 8) + bl);
            Util.CopyTo(data, 4, bx);
        }

        bool PackStream(SessionInfo sessionInfo, ushort cmd, byte[] packet, int len)
        {
            byte[] data = packet;

            var randKey = 0x1F; // (byte)new Random().Next(1, 32767);

            // Update originalData randKey
            originalData[6] = (byte)(randKey % 256);

            if (len > 9)
            {
                // Make the header

                PackHeader(sessionInfo, data, (byte)(randKey % 256));

                // ???????????????????????????????????????
                data = packet.Skip(9).ToArray();
                len -= 9;

                var origLength = BitConverter.ToUInt16(originalData, 0);
                origLength -= 11;
                Util.CopyTo(originalData, 0, origLength);
            }

            int ecx = BitConverter.ToUInt16(originalData, 2);
            int eax = originalData[6];
            eax *= ecx;
            // -> cdq ?
            byte divRes = (byte)(eax % 256);

            bool v17 = (originalData[7] & 4) == 0;
            eax = BitConverter.ToUInt16(originalData, 0);

            var tailVal = 0;
            var bytesPacked = 0;

            if (v17)
            {
                throw new Exception("why is v17 true???");
            }
            else
            {
                var i = 0;
                while (i < eax)
                {
                    tailVal += data[i];
                    data[i] = KeyTable.Table[(ushort)(data[i] + ((divRes ^ i) << 8))];
                    i++;
                }
                bytesPacked = i;
            }

            if (bytesPacked != eax || bytesPacked >= len)
            {
                // error?
                return true;
            }

            // From this point on, do NOT update data. Instead, update packet.
            for (int i = 0; i < data.Length; i++)
            {
                packet[i + 9] = data[i];
            }

            // ????????????
            // what is this packedTimes thing
            //if (packedTimes == 0)
            //{
            if ((packet[7] & 1) >= 0)
            {
                Common.UpdateKey(sessionInfo, tailVal);
            }

            if ((packet[7] & 4) >= 0)
            {
                // divRes:
                // edx = 0xF1 [actual]
                // eax = 0x570 

                // pack the tail value
                // make the sequence to be the amount of bytes packed
                var i = 0;
                var tailBytes = BitConverter.GetBytes((ushort)tailVal);
                var seq = bytesPacked;
                do
                {
                    var val = KeyTable.Table[(ushort)(tailBytes[i] + ((divRes ^ seq) << 8))];
                    packet[i + bytesPacked + 9] = val;
                    i++;
                    seq++;
                }
                while (i < 2);
            }
            //}
            
            // do we need any of the other code?

            return true;
        }

        public byte[] Pack(SessionInfo sessionInfo, ushort opcode, byte[] data)
        {
            // the header is 9 bytes

            var cons = new List<byte>();
            cons.AddRange(BitConverter.GetBytes((ushort)(data.Length + 2 + 9))); // 0-1
            cons.AddRange(BitConverter.GetBytes(opcode)); // 2-3
            cons.AddRange(BitConverter.GetBytes(sessionInfo.Client.Sequence)); // 4-5
            // 6, 7, 8: randkey, packing, checkflag
            cons.AddRange(new byte[3]);
            cons.AddRange(data);
            cons.AddRange(new byte[2]); // space for the tail flag on the client? not sure
            
            cons[7] = 0x07; // Packing = 7

            originalOpcode = opcode;
            originalData = cons.ToArray();

            var newPacket = cons.ToArray();

            PackStream(sessionInfo, opcode, newPacket, cons.Count);

            return newPacket;
        }
    }
}
