using System;
using System.Text;
using System.Collections.Generic;

namespace nMqtt
{
    internal class MqttEncoding : ASCIIEncoding
    {
        public override byte[] GetBytes(string s)
        {
            ValidateString(s);
            var stringBytes = new List<byte>
            {
                (byte)(s.Length >> 8),
                (byte)(s.Length & 0xFF)
            };
            stringBytes.AddRange(ASCII.GetBytes(s));
            return stringBytes.ToArray();
        }

        public override string GetString(byte[] bytes)
        {
            return ASCII.GetString(bytes);
        }

        public override int GetCharCount(byte[] bytes)
        {
            if (bytes.Length < 2)
                throw new ArgumentException("Length byte array must comprise 2 bytes");

            return (ushort)((bytes[0] << 8) + bytes[1]);
        }

        public override int GetByteCount(string chars)
        {
            ValidateString(chars);
            return ASCII.GetByteCount(chars) + 2;
        }

        private static void ValidateString(string s)
        {
            foreach (var c in s)
            {
                if (c > 0x7F)
                    throw new ArgumentException("The input string has extended UTF characters, which are not supported");
            }
        }
    }
}