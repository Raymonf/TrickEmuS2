using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TE2Common
{
    public class Unpacker
    {
        static byte GetValue(byte lKey, byte rKey)
        {
            return KeyTable.Table[(rKey + (lKey << 8))];
        }

        static bool CheckHeader(Header header, SessionInfo sessionInfo)
        {
            if (!sessionInfo.IsStrict || !sessionInfo.IsFirst)
            {
                return true;
            }

            byte Packing = header.Packing;
            if (Packing > 0xFu && (GetValue(header.RandKey, Packing) & 0xF) == 15)
            {
                sessionInfo.IsFirst = false;
                return true;
            }

            return false;
        }

        private static Header PacketToSHeader(byte[] data)
        {
            return new Header() {
                Len = BitConverter.ToUInt16(data, 0),
                Cmd = BitConverter.ToUInt16(data, 2),
                Seq = BitConverter.ToUInt16(data, 4),
                RandKey = data[6],
                Packing = data[7],
                CheckFlag = data[8]
            };
        }

        static Header BaseGetHeader(byte[] pRcvData, byte m_Key, Header pReason)
        {
            byte checkByte = pRcvData[7];
            if (checkByte > 0xFu)
            {
                byte packing = pRcvData[7];
                byte a8 = pRcvData[6];

                int pos = (a8 << 8) + packing;

                byte v5 = KeyTable.Table[pos];

                if (v5 > 0xFu || (v5 & 2) == 0)
                {
                    if (pReason != null)
                    {
                        pReason.Len = 0;
                        pReason.Packing = v5;
                    }
                    return null;
                }

                ushort sequence = 0;

                int ecx = pRcvData[7];
                int eax = pRcvData[3];

                eax ^= pRcvData[7];
                ecx = pRcvData[5];
                eax <<= 8;
                eax += ecx;
                byte a = KeyTable.Table[(ushort)eax];
                sequence = (ushort)(a << 8);

                eax = pRcvData[2];
                ecx = pRcvData[4];
                eax ^= m_Key;
                eax <<= 8;
                eax += ecx;
                byte b = KeyTable.Table[eax];
                sequence += b;

                //Console.WriteLine("Sequence: {0}", Util.ByteToHex(sequence));


                //////////////////////////////////////////

                ushort opcode = 0;

                // left
                ecx = pRcvData[3];
                eax = pRcvData[1];
                eax ^= m_Key;
                eax <<= 8;
                eax += ecx;
                opcode = KeyTable.Table[(ushort)eax];
                opcode <<= 8;

                // right
                ecx = pRcvData[0];
                eax = pRcvData[6];
                eax ^= ecx;
                ecx = pRcvData[2];
                eax <<= 8;
                eax += ecx;
                opcode += KeyTable.Table[(ushort)eax];

                //Console.WriteLine("Opcode: {0}", Util.ByteToHex(opcode));

                //////////////////////////////////////////

                ushort length = 0;

                ecx = pRcvData[7]; // ok
                eax = pRcvData[6];
                eax ^= ecx;
                eax <<= 8;
                eax += pRcvData[1];
                length = (ushort)((KeyTable.Table[(ushort)eax]) << 8);

                eax = pRcvData[7];
                ecx = pRcvData[0];
                eax ^= m_Key;
                eax <<= 8;
                eax += ecx;

                length += KeyTable.Table[(ushort)eax];

                //Console.WriteLine("Length (incl. dummy): {0}", Util.ByteToHex(length));

                // Update Packing
                pRcvData[7] = (byte)(v5 ^ 2);

                Util.CopyTo(pRcvData, 0, length);
                Util.CopyTo(pRcvData, 2, opcode);
                Util.CopyTo(pRcvData, 4, sequence);
            }

            if (BitConverter.ToUInt16(pRcvData, 0) >= 0xBu && BitConverter.ToInt32(pRcvData, 0) < 0x7FFFFFFFu && Common.MakeChecksum(pRcvData, m_Key) == pRcvData[8]) {
                return PacketToSHeader(pRcvData);
            }

            if (pReason != null)
            {
                pReason.Packing = pRcvData[7];
                pReason.Len = pRcvData[0];
            }

            return null;
        }


        static bool ExcludeDummy(byte[] pRcvData)
        {
            byte Packing = pRcvData[7];

            if ((Packing & 8) > 0)
            {
                ushort p6 = pRcvData[6];
                ushort v2 = (ushort)(BitConverter.ToUInt16(pRcvData, 0) - (p6 % 13)); // _mUse

                if (v2 < 0xB)
                {
                    return false;
                }

                // Update the length
                /*
                var v2b = BitConverter.GetBytes(v2);
                pRcvData[0] = v2b[0];
                pRcvData[1] = v2b[1];
                */
                Util.CopyTo(pRcvData, 0, v2);

                pRcvData[7] = (byte)(Packing ^ 8);
            }

            return true;
        }

        static bool UnpackData(SessionInfo sessionInfo, byte[] pRcvData)
        {
            var header = PacketToSHeader(pRcvData);
            byte v4 = (byte)(header.Cmd * header.RandKey % 256);
            header.Len -= 2;

            int i = 0;
            ushort len = header.Len;
            ushort calcTailValue = 0;

            if ((header.Packing & 4) > 0)
            {
                if (len != 9) // TODO: Is this supposed to be len > 9?
                {
                    do
                    {
                        ushort val = pRcvData[i + 9];
                        byte v7 = KeyTable.Table[(ushort)(val + ((v4 ^ i) << 8))];
                        pRcvData[i + 9] = v7;
                        calcTailValue += v7;
                        i++;
                    }
                    while (i < header.Len - 9);
                }

                var j = 2;

                do
                {
                    ushort val = pRcvData[i + 9];
                    byte v1 = KeyTable.Table[(ushort)(val + ((v4 ^ i) << 8))];
                    pRcvData[i + 9] = v1;
                    i++;
                    j--;
                }
                while (j > 0);

                header.Packing ^= 4;
            }
            else
            {
                i = len - 7;
                calcTailValue = (ushort)(len - 9);
            }

            ushort pktTailVal = BitConverter.ToUInt16(pRcvData, i + 7);

            if (calcTailValue != pktTailVal)
            {
                return false;
            }

            byte al = header.Packing;

            if ((al & 1) > 0)
            {
                pRcvData[7] = 0;
                header.Packing = (byte)(al ^ 1);

                Common.UpdateKey(sessionInfo, calcTailValue);
            }

            return true;
        }

        public static int Unpack(SessionInfo sessionInfo, byte[] pRcvData)
        {
            ushort fullLength = 9;
            sessionInfo.Mutex.WaitOne();

            try
            {
                var header = UnpackHeader(pRcvData, sessionInfo);
                if (header == null)
                    throw new Exception("Header could not be unpacked");

                fullLength = header.Len;

                bool excludeDummy = ExcludeDummy(pRcvData);
                if (!excludeDummy)
                    throw new Exception("Dummy could not be excluded");

                bool unpackData = UnpackData(sessionInfo, pRcvData);
                if (!unpackData)
                    throw new Exception("Data could not be unpacked");
            }
            finally
            {

                sessionInfo.Mutex.ReleaseMutex();
            }

            return fullLength;
        }

        public static Header UnpackHeader(byte[] pRcvData, SessionInfo sessionInfo)
        {
            var checkHeader = CheckHeader(PacketToSHeader(pRcvData), sessionInfo);
            //Console.WriteLine("CheckHeader returned {0}", checkHeader);

            if (!checkHeader)
                throw new Exception("Header was invalid");

            return BaseGetHeader(pRcvData, sessionInfo.Key, null);
        }
    }
}
