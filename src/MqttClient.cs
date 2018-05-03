using System;
using System.Threading;
using System.Diagnostics;
using nMqtt.Messages;

namespace nMqtt
{
    /// <summary>
    /// Mqtt客户端
    /// </summary>
    public sealed class MqttClient : IDisposable
    {
        Timer pingTimer;
        MqttConnection conn;
        readonly AutoResetEvent connResetEvent;
        public Action<string, byte[]> MessageReceived;

        /// <summary>
        /// 客户端标识
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 服务器地址
        /// </summary>
        public string Server { get; set; } = "localhost";
        /// <summary>
        /// 服务器端口
        /// </summary>
        public int Port { get; set; } = 1883;

        public short KeepAlive { get; set; } = 60;

        public bool CleanSession { get; set; } = true;

        public ConnectionState ConnectionState { get; private set; }

        public MqttClient(string server, string clientId = default(string))
        {
            Server = server;
            if (string.IsNullOrEmpty(clientId))
                clientId = MqttUtils.NextId();
			ClientId = clientId;
			conn = new MqttConnection();
            conn.Recv += DecodeMessage;
            connResetEvent = new AutoResetEvent(false);
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public ConnectionState Connect()
        {
            return Connect(string.Empty, string.Empty);
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public ConnectionState Connect(string username = default(string), string password = default(string))
        {
            ConnectionState = ConnectionState.Connecting;
            conn.Connect(Server, Port);

			var msg = new ConnectMessage
			{
				ClientId = ClientId,
				CleanSession = CleanSession
			};
			if (!string.IsNullOrEmpty(username))
            {
                msg.UsernameFlag = true;
                msg.UserName = username;
            }
            if(!string.IsNullOrEmpty(password))
            {
                msg.PasswordFlag = true;
                msg.Password = password;
            }
            msg.KeepAlive = KeepAlive;
            conn.SendMessage(msg);

            if (!connResetEvent.WaitOne(5000, false))
            {
                ConnectionState = ConnectionState.Disconnecting;
                Dispose();
                ConnectionState = ConnectionState.Disconnected;
                return ConnectionState;
            }

            if (ConnectionState == ConnectionState.Connected)
            {
                pingTimer = new Timer((state) =>
                {
                    conn.SendMessage(new PingReqMessage());
                }, null, KeepAlive * 1000, KeepAlive * 1000);
            }

            return ConnectionState;
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="data">数据</param>
        /// <param name="qos">服务质量等级</param>
        public void Publish(string topic, byte[] data, Qos qos = Qos.AtMostOnce)
        {
            var msg = new PublishMessage();
            msg.FixedHeader.Qos = qos;
            msg.MessageIdentifier = 0;
            msg.TopicName = topic;
            msg.Payload = data;
            conn.SendMessage(msg);
        }

        /// <summary>
        /// 订阅主题
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="qos"></param>
        public void Subscribe(string topic, Qos qos = Qos.AtMostOnce)
        {
            var msg = new SubscribeMessage();
            msg.Subscribe(topic, qos);
            conn.SendMessage(msg);
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topic"></param>
        public void Unsubscribe(string topic)
        {
            var msg = new UnsubscribeMessage();
            msg.FixedHeader.Qos = Qos.AtLeastOnce;
            msg.Unsubscribe(topic);
            conn.SendMessage(msg);
        }

        void DecodeMessage(byte[] buffer)
        {
            var msg = MqttMessage.DecodeMessage(buffer);
            Debug.WriteLine("onRecv:{0}", msg.FixedHeader.MessageType);
            switch (msg.FixedHeader.MessageType)
            {
                case MessageType.CONNACK:
                    var connAckMsg = (ConnAckMessage)msg;
                    if (connAckMsg.ConnectReturnCode == ConnectReturnCode.BrokerUnavailable ||
                       connAckMsg.ConnectReturnCode == ConnectReturnCode.IdentifierRejected ||
                       connAckMsg.ConnectReturnCode == ConnectReturnCode.UnacceptedProtocolVersion ||
                       connAckMsg.ConnectReturnCode == ConnectReturnCode.NotAuthorized ||
                       connAckMsg.ConnectReturnCode == ConnectReturnCode.BadUsernameOrPassword)
                    {
                        ConnectionState = ConnectionState.Disconnecting;
                        Dispose();
                        conn = null;
                        ConnectionState = ConnectionState.Disconnected;
                    }
                    else
                    {
                        ConnectionState = ConnectionState.Connected;
                    }
                    connResetEvent.Set();
                    break;
                case MessageType.PUBLISH:
                    var pubMsg = (PublishMessage)msg;
                    string topic = pubMsg.TopicName;
                    var data = pubMsg.Payload;
                    if(pubMsg.FixedHeader.Qos == Qos.AtLeastOnce)
                    {
                        var ackMsg = new PublishAckMessage
                        {
                            MessageIdentifier = pubMsg.MessageIdentifier
                        };
                        conn.SendMessage(ackMsg);
                    }
                    OnMessageReceived(topic, data);
                    break;
                case MessageType.PUBACK:
                    var pubAckMsg = (PublishAckMessage)msg;
                    Debug.WriteLine("PUBACK MessageIdentifier:" + pubAckMsg.MessageIdentifier);
                    break;
                case MessageType.PUBREC:
                    break;
                case MessageType.PUBREL:
                    break;
                case MessageType.PUBCOMP:
                    break;
                case MessageType.SUBSCRIBE:
                    break;
                case MessageType.SUBACK:
                    break;
                case MessageType.UNSUBSCRIBE:
                    break;
                case MessageType.UNSUBACK:
                    break;
                case MessageType.PINGREQ:
                    conn.SendMessage(new PingRespMessage());
                    break;
                case MessageType.DISCONNECT:
                    Disconnect();
                    break;
            }
        }

        void OnMessageReceived(string topic, byte[] data)
        {
			MessageReceived?.Invoke(topic, data);
		}

        void Disconnect()
        {
            if (conn.Recv != null) 
                conn.Recv -= DecodeMessage;
        }

        void Close()
        {
            if (ConnectionState == ConnectionState.Connecting)
            {
                // TODO: Decide what to do if the caller tries to close a connection that is in the process of being connected.
            }
            if (ConnectionState == ConnectionState.Connected)
            {
                Disconnect();
            }
        }

        public void Dispose()
        {
            if (conn != null)
            {
                Close();
                if (conn != null)
                {
                    //connection.Dispose();
                }
            }
           if (pingTimer != null)
               pingTimer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
