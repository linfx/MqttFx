using DotNetty.Buffers;
using System;
using System.IO;

namespace nMqtt
{
    internal static class MqttExtensions
    {
        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="buffer"></param>
        public static void Write(this Stream stream, byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 写入两字节
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="value"></param>
        public static void WriteShort(this Stream stream, short value)
        {
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value & 0xFF));
        }

        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="stream">The stream containing the string to write.</param>
        /// <param name="value">The string to write.</param>
        public static void WriteString(this Stream stream, string value)
        {
            System.Text.Encoding enc = new MqttEncoding();
            byte[] stringBytes = enc.GetBytes(value);
            stream.Write(stringBytes, 0, stringBytes.Length);
        }

        /// <summary>
        /// 写入字符串
        /// </summary>
        /// <param name="buffer">The stream containing the string to write.</param>
        /// <param name="value">The string to write.</param>
        public static void WriteString(this IByteBuffer buffer, string value)
        {
            System.Text.Encoding enc = new MqttEncoding();
            byte[] stringBytes = enc.GetBytes(value);
            buffer.WriteBytes(stringBytes, 0, stringBytes.Length);
        }

        /// <summary>
        /// 读取两字节
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static short ReadShort(this Stream stream)
        {
            byte high, low;
            high = (byte)stream.ReadByte();
            low = (byte)stream.ReadByte();
            return (short)((high << 8) + low);
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ReadString(this Stream stream)
        {
            // read and check the length
            var lengthBytes = new byte[2];
            var bytesRead = stream.Read(lengthBytes, 0, 2);
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
                bytesRead = stream.Read(readBuffer, 0, nextReadSize);
                Array.Copy(readBuffer, 0, stringBytes, totalRead, bytesRead);
                totalRead += bytesRead;
            }

            return enc.GetString(stringBytes);
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static string ReadString(this IByteBuffer buffer)
        {
            // read and check the length
            var lengthBytes = new byte[2];
            var bytesRead = buffer.ReadBytes(lengthBytes, 0, 2);
            //if (bytesRead < 2)
            //{
            //    throw new ArgumentException(
            //        "The stream did not have enough bytes to describe the length of the string",
            //        "stringStream");
            //}

            var enc = new MqttEncoding();
            var stringLength = (ushort)enc.GetCharCount(lengthBytes);

            // read the bytes from the string, validate we have enough etc.
            var stringBytes = new byte[stringLength];
            var readBuffer = new byte[1 << 10]; // 1KB read buffer
            var totalRead = 0;

            // Keep reading until we have all. Intentionally synchronous
            //while (totalRead < stringLength)
            //{
            //    var remainingBytes = stringLength - totalRead;
            //    var nextReadSize = remainingBytes > readBuffer.Length ? readBuffer.Length : remainingBytes;
            //    bytesRead = buffer.Read(readBuffer, 0, nextReadSize);
            //    Array.Copy(readBuffer, 0, stringBytes, totalRead, bytesRead);
            //    totalRead += bytesRead;
            //}

            return enc.GetString(stringBytes);
        }

        /// <summary>
        /// 布尔转字节
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte ToByte(this bool input)
        {
            return input ? (byte)1 : (byte)0;
        }
    }
}