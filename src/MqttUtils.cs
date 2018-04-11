using System;
using System.IO;

namespace nMqtt
{
    internal static class MqttUtils
    {
        public static void Write(this Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        public static void WriteShort(this Stream stream, short value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value & 0xFF));
        }

        public static void WriteShort(this Stream stream, int value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value & 0xFF));
        }

        public static short ReadShort(this Stream stream)
        {
            byte high, low;
            high = (byte)stream.ReadByte();
            low = (byte)stream.ReadByte();
            return (short)((high << 8) + low);
        }

        public static string ReadString(this Stream stringStream)
        {
            // read and check the length
            var lengthBytes = new byte[2];
            var bytesRead = stringStream.Read(lengthBytes, 0, 2);
            if (bytesRead < 2)
            {
                throw new ArgumentException(
                    "The stream did not have enough bytes to describe the length of the string",
                    "stringStream");
            }

            var enc = new MqttEncoding();
            var stringLength = (ushort)enc.GetCharCount(lengthBytes);

            // read the bytes from the string, validate we have enough etc.
            var stringBytes = new byte[stringLength];
            var readBuffer = new byte[1 << 10]; // 1KB read buffer
            var totalRead = 0;

            // Keep reading until we have all. Intentionally synchronous
            while (totalRead < stringLength)
            {
                var remainingBytes = stringLength - totalRead;
                var nextReadSize = remainingBytes > readBuffer.Length ? readBuffer.Length : remainingBytes;
                bytesRead = stringStream.Read(readBuffer, 0, nextReadSize);
                Array.Copy(readBuffer, 0, stringBytes, totalRead, bytesRead);
                totalRead += bytesRead;
            }

            return enc.GetString(stringBytes);
        }

        /// <summary>
        ///     Writes the MQTT string.
        /// </summary>
        /// <param name="stringStream">The stream containing the string to write.</param>
        /// <param name="value">The string to write.</param>
        public static void WriteString(this Stream stringStream, string value)
        {
            System.Text.Encoding enc = new MqttEncoding();
            byte[] stringBytes = enc.GetBytes(value);
            stringStream.Write(stringBytes, 0, stringBytes.Length);
        }

        public static byte BoolToByte(bool b)
        {
            switch (b)
            {
                case true:
                    return 1;
                default:
                    return 0;
            }
        }

        public static byte ToByte(this bool b)
        {
            switch (b)
            {
                case true:
                    return 1;
                default:
                    return 0;
            }
        }
    }
}
