using System;
using System.IO;

namespace nMqtt
{
    internal static partial class MqttUtils
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
        /// 布尔转字节
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte ToByte(this bool input)
        {
            return input ? (byte)1 : (byte)0;
        }
    }

    internal static partial class MqttUtils
    {
        public static string NextId()
        {
            return Snowflake.Instance().GetId().ToString();
        }
    }

    /// <summary>
    /// 雪花（snowflake）算法
    /// </summary>
    class Snowflake
    {
        private static long machineId;//机器ID
        private static long datacenterId = 0L;//数据ID
        private static long sequence = 0L;//计数从零开始
        private static long twepoch = 687888001020L; //唯一时间随机量
        private static long machineIdBits = 5L; //机器码字节数
        private static long datacenterIdBits = 5L;//数据字节数
        public static long maxMachineId = -1L ^ -1L << (int)machineIdBits; //最大机器ID
        private static long maxDatacenterId = -1L ^ (-1L << (int)datacenterIdBits);//最大数据ID
        private static long sequenceBits = 12L; //计数器字节数，12个字节用来保存计数码        
        private static long machineIdShift = sequenceBits; //机器码数据左移位数，就是后面计数器占用的位数
        private static long datacenterIdShift = sequenceBits + machineIdBits;
        private static long timestampLeftShift = sequenceBits + machineIdBits + datacenterIdBits; //时间戳左移动位数就是机器码+计数器总字节数+数据字节数
        public static long sequenceMask = -1L ^ -1L << (int)sequenceBits; //一微秒内可以产生计数，如果达到该值则等到下一微妙在进行生成
        private static long lastTimestamp = -1L;//最后时间戳
        private static object syncRoot = new object();//加锁对象
        static Snowflake snowflake;

        public static Snowflake Instance()
        {
            if (snowflake == null)
                snowflake = new Snowflake();
            return snowflake;
        }

        public Snowflake()
        {
            Snowflakes(0L, -1);
        }

        public Snowflake(long machineId)
        {
            Snowflakes(machineId, -1);
        }

        public Snowflake(long machineId, long datacenterId)
        {
            Snowflakes(machineId, datacenterId);
        }

        private void Snowflakes(long machineId, long datacenterId)
        {
            if (machineId >= 0)
            {
                if (machineId > maxMachineId)
                {
                    throw new Exception("机器码ID非法");
                }
                Snowflake.machineId = machineId;
            }
            if (datacenterId >= 0)
            {
                if (datacenterId > maxDatacenterId)
                {
                    throw new Exception("数据中心ID非法");
                }
                Snowflake.datacenterId = datacenterId;
            }
        }

        /// <summary>
        /// 生成当前时间戳
        /// </summary>
        /// <returns>毫秒</returns>
        private static long GetTimestamp()
        {
            //return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// 获取下一微秒时间戳
        /// </summary>
        /// <param name="lastTimestamp"></param>
        /// <returns></returns>
        private static long GetNextTimestamp(long lastTimestamp)
        {
            long timestamp = GetTimestamp();
            if (timestamp <= lastTimestamp)
            {
                timestamp = GetTimestamp();
            }
            return timestamp;
        }

        /// <summary>
        /// 获取长整形的ID
        /// </summary>
        /// <returns></returns>
        public long GetId()
        {
            lock (syncRoot)
            {
                long timestamp = GetTimestamp();
                if (lastTimestamp == timestamp)
                { //同一微妙中生成ID
                    sequence = (sequence + 1) & sequenceMask; //用&运算计算该微秒内产生的计数是否已经到达上限
                    if (sequence == 0)
                    {
                        //一微妙内产生的ID计数已达上限，等待下一微妙
                        timestamp = GetNextTimestamp(lastTimestamp);
                    }
                }
                else
                {
                    //不同微秒生成ID
                    sequence = 0L;
                }
                if (timestamp < lastTimestamp)
                {
                    throw new Exception("时间戳比上一次生成ID时时间戳还小，故异常");
                }
                lastTimestamp = timestamp; //把当前时间戳保存为最后生成ID的时间戳
                long Id = ((timestamp - twepoch) << (int)timestampLeftShift)
                    | (datacenterId << (int)datacenterIdShift)
                    | (machineId << (int)machineIdShift)
                    | sequence;
                return Id;
            }
        }
    }
}