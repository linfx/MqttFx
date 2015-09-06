using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Diagnostics;
using nMqtt.Messages;

namespace nMqtt
{
    internal sealed class MqttConnection
    {
        Socket socket;
        int m_nConnection = 1;
        public Action<byte[]> Recv;

        /// <summary>
        /// Socket异步对象池
        /// </summary>
        SocketAsyncEventArgsPool socketAsynPool;

        public MqttConnection()
        {
            const int receiveBufferSize = 4096;
            socketAsynPool = new SocketAsyncEventArgsPool(m_nConnection);
            var bufferManager = new BufferManager(receiveBufferSize * m_nConnection, receiveBufferSize);
            bufferManager.InitBuffer();

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

        public void Connect(string server, int port = 1883)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(server, port);
            socket.ReceiveAsync(socketAsynPool.Pop());
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

        void ProcessRecv(SocketAsyncEventArgs e)
        {
            Debug.WriteLine("----------------------- ProcessRecv:{0}", e.BytesTransferred);
            var token = e.UserToken as RecvToken;
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                var buffer = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, e.Offset, buffer, 0, buffer.Length);

                token.Message.AddRange(buffer);

                if (token.IsReadComplete)
                {
                    if (Recv != null)
                        Recv(token.Message.ToArray());
                    token.Reset();
                }

                socket.ReceiveAsync(e);
            }
            else
            {
                token.Reset();
                //socketAsynPool.Push(e);
                //socket.ReceiveAsync(e);
            }
        }

        public void SendMessage(MqttMessage message)
        {
            Debug.WriteLine("onSend:{0}", message.FixedHeader.MessageType);
            using (var stream = new MemoryStream())
            {
                message.Encode(stream);
                stream.Seek(0, SeekOrigin.Begin);
                Send(stream.ToArray());
            }
        }

        void Send(byte[] message)
        {
            try
            {
                socket.Send(message, SocketFlags.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("*********************");
                Debug.WriteLine(ex);
                Debug.WriteLine("*********************");
            }
        }

        class RecvToken
        {
            /// <summary>
            /// The read buffer size from the network
            /// </summary>
            public const int BufferSize = 4096;

            /// <summary>
            ///     The total bytes expected to be read from from the header of content
            /// </summary>
            public int Count
            {
                get
                {
                    if (Message != null && Message.Count >= 2)
                    {
                        int offset = 1;
                        byte encodedByte;
                        var multiplier = 1;
                        var remainingLength = 0;
                        do
                        {
                            encodedByte = Message[offset];
                            remainingLength += encodedByte & 0x7f * multiplier;
                            multiplier *= 0x80;
                        } while ((++offset <=4) && (encodedByte & 0x80) != 0);

                        return remainingLength + offset;
                    }
                    return 0;
                }
            }

            /// <summary>
            ///     The bytes associated with the message being read.
            /// </summary>
            public List<byte> Message { get; set; } = new List<byte>();

            /// <summary>
            ///     The buffer the last stream read wrote into.
            /// </summary>
            public byte[] Buffer;

            /// <summary>
            /// A boolean that indicates whether the message read is complete 
            /// </summary>
            public bool IsReadComplete
            {
                get { return Message.Count >= Count; }
            }

            public void Reset()
            {
                Buffer = new byte[BufferSize];
                Message.Clear();
            }
        }
    }

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

    internal sealed class SocketAsyncEventArgsPool
    {
        private readonly Stack<SocketAsyncEventArgs> pool;

        internal SocketAsyncEventArgsPool(int capacity)
        {
            pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        internal void Push(SocketAsyncEventArgs item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            lock (pool)
            {
                pool.Push(item);
            }
        }

        internal SocketAsyncEventArgs Pop()
        {
            lock (pool)
                return pool.Pop();
        }

        internal int Count { get { return pool.Count; } }
    }

    internal sealed class BufferManager
    {
        readonly int m_numBytes;
        byte[] m_buffer;
        readonly Stack<int> m_freeIndexPool;
        int m_currentIndex;
        readonly int m_bufferSize;

        public BufferManager(int totalBytes, int bufferSize)
        {
            m_numBytes = totalBytes;
            m_currentIndex = 0;
            m_bufferSize = bufferSize;
            m_freeIndexPool = new Stack<int>();
        }

        /// <summary>
        /// Allocates buffer space used by the buffer pool
        /// </summary>
        public void InitBuffer()
        {
            // create one big large buffer and divide that out to each SocketAsyncEventArg object
            m_buffer = new byte[m_numBytes];
        }

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
