using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Diagnostics;
using nMqtt.Messages;
using System.Threading.Tasks;

namespace nMqtt
{
    internal sealed class MqttConnection
    {
        Socket _socket;

        /// <summary>
        /// 最大连接数
        /// </summary>
        readonly int m_nConnection = 10;

        public Action<byte[]> Recv;

        /// <summary>
        /// Socket异步对象池
        /// </summary>
        readonly SocketAsyncEventArgsPool socketAsynPool;

        public MqttConnection()
        {
            const int receiveBufferSize = 4096;
            var bufferManager = new BufferManager(receiveBufferSize * m_nConnection, receiveBufferSize);
            bufferManager.ResetBuffer();
            socketAsynPool = new SocketAsyncEventArgsPool(m_nConnection);

            //按照连接数建立读写对象
            for (int i = 0; i < m_nConnection; i++)
            {
                var args = new SocketAsyncEventArgs();
                args.Completed += IO_Completed;
                args.UserToken = new RecvToken();
                bufferManager.SetBuffer(args);
                socketAsynPool.Push(args);
            }
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        public async Task ConnectAsync(string server, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _socket.ConnectAsync(server, port);
            _socket.ReceiveAsync(socketAsynPool.Pop());
        }

        void ProcessRecv(SocketAsyncEventArgs e)
        {
            var token = e.UserToken as RecvToken;
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                var buffer = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, e.Offset, buffer, 0, buffer.Length);

                token.Buffer.AddRange(buffer);

                if (token.IsReadComplete)
                {
                    Recv?.Invoke(token.Buffer.ToArray());
                    token.Reset();
                }

                _socket.ReceiveAsync(e);
            }
            else
            {
                token.Reset();
                //socketAsynPool.Push(e);
                //socket.ReceiveAsync(e);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(MqttMessage message)
        {
            Debug.WriteLine("onSend:{0}", message.FixedHeader.MessageType);
            using (var stream = new MemoryStream())
            {
                message.Encode(stream);
                stream.Seek(0, SeekOrigin.Begin);
                _socket.Send(stream.ToArray(), SocketFlags.None);
            }
        }

        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessRecv(e);
                    break;
                default:
                    throw new ArgumentException("nError in I/O Completed");
            }
        }

        class RecvToken
        {
            public List<byte> Buffer { get; set; } = new List<byte>();

            public int Count
            {
                get
                {
                    if (Buffer != null && Buffer.Count >= 2)
                    {
                        int offset = 1;
                        byte encodedByte;
                        var multiplier = 1;
                        var remainingLength = 0;

                        do
                        {
                            encodedByte = Buffer[offset];
                            remainingLength += encodedByte & 0x7f * multiplier;
                            multiplier *= 0x80;
                        } while ((++offset <= 4) && (encodedByte & 0x80) != 0);

                        return remainingLength + offset;
                    }
                    return 0;
                }
            }

            /// <summary>
            /// A boolean that indicates whether the message read is complete 
            /// </summary>
            public bool IsReadComplete
            {
                get { return Buffer.Count >= Count; }
            }

            public void Reset()
            {
                Buffer.Clear();
            }
        }
    }

    /// <summary>
    /// 连接状态
    /// </summary>
    public enum ConnectionState
    {
        /// <summary>
        ///     The MQTT Connection is in the process of disconnecting from the broker.
        /// </summary>
        Disconnecting,
        /// <summary>
        ///     The MQTT Connection is not currently connected to any broker.
        /// </summary>
        Disconnected,
        /// <summary>
        ///     The MQTT Connection is in the process of connecting to the broker.
        /// </summary>
        Connecting,
        /// <summary>
        ///     The MQTT Connection is currently connected to the broker.
        /// </summary>
        Connected,
        /// <summary>
        ///     The MQTT Connection is faulted and no longer communicating with the broker.
        /// </summary>
        Faulted
    }

    /// <summary>
    /// Socket异步对象池
    /// </summary>
    internal sealed class SocketAsyncEventArgsPool
    {
        readonly Stack<SocketAsyncEventArgs> pool;

        internal SocketAsyncEventArgsPool(int capacity) => pool = new Stack<SocketAsyncEventArgs>(capacity);

        internal void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            lock (pool)
                pool.Push(item);
        }

        internal SocketAsyncEventArgs Pop()
        {
            lock (pool)
                return pool.Pop();
        }

        internal int Count { get { return pool.Count; } }
    }

    /// <summary>
    /// 缓存管理
    /// </summary>
    internal sealed class BufferManager
    {
        byte[] m_buffer;
        int m_currentIndex;
        readonly int m_numBytes;
        readonly int m_bufferSize;
        readonly Stack<int> m_freeIndexPool;

        public BufferManager(int totalBytes, int bufferSize)
        {
            m_numBytes = totalBytes;
            m_currentIndex = 0;
            m_bufferSize = bufferSize;
            m_freeIndexPool = new Stack<int>();
        }

        /// <summary>
        /// Allocates buffer space used by the buffer pool
        /// create one big large buffer and divide that out to each SocketAsyncEventArg object
        /// </summary>
        public void ResetBuffer() => m_buffer = new byte[m_numBytes];

        /// <summary>
        /// Assigns a buffer from the buffer pool to the specified SocketAsyncEventArgs object
        /// </summary>
        /// <returns>true if the buffer was successfully set, else false</returns>
        public bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (m_freeIndexPool.Count > 0)
            {
                args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_bufferSize);
            }
            else
            {
                if ((m_numBytes - m_bufferSize) < m_currentIndex)
                    return false;
                args.SetBuffer(m_buffer, m_currentIndex, m_bufferSize);
                m_currentIndex += m_bufferSize;
            }
            return true;
        }

        /// <summary>
        /// Removes the buffer from a SocketAsyncEventArg object.  This frees the buffer back to the 
        /// buffer pool
        /// </summary>
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            m_freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}