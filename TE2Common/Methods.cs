using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TE2Common
{
    public class Methods
    {
        public static string Sep(string str, string delim)
        {
            int len = str.IndexOf(delim);
            if (len > 0)
            {
                return str.Substring(0, len);
            }
            return "";
        }

        public static string GetString(byte[] bytes, int i)
        {
            return Constants.Encoding.GetString(bytes);
        }

        public static ushort ReadUshort(byte[] bytes, int pos)
        {
            byte[] ba = new byte[2] { bytes[pos], bytes[pos + 1] };
            return BitConverter.ToUInt16(ba.Reverse().ToArray(), 0);
        }

        public static string CleanString(string str)
        {
            return str.TrimEnd(new char[] { (char)0 }).Replace("\x00", "").Replace("\u0000", "");
        }

        // From http://stackoverflow.com/questions/8041343/how-to-split-a-byte
        // Author: driis http://stackoverflow.com/users/13627/driis
        public static IEnumerable<byte[]> Split(byte splitByte, byte[] buffer)
        {
            List<byte> bytes = new List<byte>();
            foreach (byte b in buffer)
            {
                if (b != splitByte)
                    bytes.Add(b);
                else
                {
                    yield return bytes.ToArray();
                    bytes.Clear();
                }
            }
            yield return bytes.ToArray();
        }

        public static string ReadTerminatedString(byte[] array, int index = 0)
        {
            var bytes = new List<byte>();
            //StringBuilder builder = new StringBuilder();

            var i = index;
            
            while (true)
            {
                if(array[i] == 0x00 || i >= array.Length)
                {
                    return Constants.Encoding.GetString(bytes.ToArray());
                }

                //builder.Append((char)array[i]);
                bytes.Add(array[i]);
                i++;
            }
        }

        public static byte[] ReadTerminatedStringToBytes(byte[] array, int index = 0)
        {
            var bytes = new List<byte>();
            //StringBuilder builder = new StringBuilder();

            var i = index;

            while (true)
            {
                if (array[i] == 0x00 || i >= array.Length)
                {
                    return bytes.ToArray();
                }

                //builder.Append((char)array[i]);
                bytes.Add(array[i]);
                i++;
            }
        }

        public static string Utf8Convert(string s)
        {
            return Constants.Encoding.GetString(Encoding.Convert(Encoding.UTF8, Constants.Encoding, Encoding.UTF8.GetBytes(s)));
        }

        public static ulong DateTimeToUnix(DateTime time)
        {
            return (ulong)(time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
        }
    }
}
