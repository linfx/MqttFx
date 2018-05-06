using System;
using System.Threading;
using System.Diagnostics;
using nMqtt.Messages;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace nMqtt
{
    /// <summary>
    /// Mqtt客户端
    /// </summary>
    public sealed class MqttClient : IDisposable
    {
        ILogger _logger;
        Timer _pingTimer;
        MqttConnection _conn;
        readonly AutoResetEvent connResetEvent;
        public Action<MqttMessage> OnMessageReceived;

        public MqttClient(string server, string clientId = default(string), ILogger logger = default(ILogger))
        {
            Server = server;
            if (string.IsNullOrEmpty(clientId))
                clientId = MqttUtils.NextId();
            ClientId = clientId;
            _logger = logger ?? NullLogger<MqttClient>.Instance;
            _conn = new MqttConnection();
            _conn.Recv += ProcesMessage;
            connResetEvent = new AutoResetEvent(false);
        }


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
        /// <summary>
        /// 连接状态
        /// </summary>
        public ConnectionState ConnectionState { get; private set; }

        /// <summary>
        /// 连接
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public async Task<ConnectionState> ConnectAsync(string username = default(string), string password = default(string))
        {
            ConnectionState = ConnectionState.Connecting;
            await _conn.ConnectAsync(Server, Port);

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
            if (!string.IsNullOrEmpty(password))
            {
                msg.PasswordFlag = true;
                msg.Password = password;
            }
            msg.KeepAlive = KeepAlive;
            _conn.SendMessage(msg);

            if (!connResetEvent.WaitOne(5000, false))
            {
                ConnectionState = ConnectionState.Disconnecting;
                Dispose();
                ConnectionState = ConnectionState.Disconnected;
                return ConnectionState;
            }

            if (ConnectionState == ConnectionState.Connected)
            {
                _pingTimer = new Timer((state) =>
                {
                    _conn.SendMessage(new PingReqMessage());
                }, null, KeepAlive * 1000, KeepAlive * 1000);
            }

            return ConnectionState;
        }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="buffer"></param>
        void ProcesMessage(byte[] buffer)
        {
            try
            {
                var message = MessageFactory.CreateMessage(buffer);
                Debug.WriteLine("onRecv:{0}", message.FixedHeader.MessageType);
                switch (message)
                {
                    #region CONNACK
                    case ConnAckMessage msg:
                        if (msg.ConnectReturnCode == ConnectReturnCode.BrokerUnavailable ||
                           msg.ConnectReturnCode == ConnectReturnCode.IdentifierRejected ||
                           msg.ConnectReturnCode == ConnectReturnCode.UnacceptedProtocolVersion ||
                           msg.ConnectReturnCode == ConnectReturnCode.NotAuthorized ||
                           msg.ConnectReturnCode == ConnectReturnCode.BadUsernameOrPassword)
                        {
                            ConnectionState = ConnectionState.Disconnecting;
                            Dispose();
                            _conn = null;
                            ConnectionState = ConnectionState.Disconnected;
                        }
                        else
                        {
                            ConnectionState = ConnectionState.Connected;
                        }
                        connResetEvent.Set();
                        break;
                    #endregion
                    #region PINGREQ
                    case PingReqMessage msg:
                        _conn.SendMessage(new PingRespMessage());
                        return;
                    #endregion
                    #region DISCONNECT
                    case DisconnectMessage msg:
                        Disconnect();
                        break; 
                    #endregion
                    #region PUBLISH
                    case PublishMessage msg:
                        if (msg.FixedHeader.Qos == Qos.AtLeastOnce)
                        {
                            _conn.SendMessage(new PublishAckMessage(msg.MessageIdentifier));
                        }
                        else if (msg.FixedHeader.Qos == Qos.ExactlyOnce)
                        {
                            _conn.SendMessage(new PublishRecMessage(msg.MessageIdentifier));
                        }
                        break;
                    #endregion
                    #region PUBACK
                    case PublishAckMessage msg:
                        Debug.WriteLine("Todo Delete(Msg)");
                        //_logger.LogDebug("Todo Delete(Msg)");
                        return;
                    #endregion
                    //case MessageType.PUBREC:
                    //    break;
                    //case MessageType.PUBREL:
                    //    OnMessageReceived?.Invoke(message);
                    //    break;
                    //case MessageType.PUBCOMP:
                    //    break;
                    //case MessageType.SUBSCRIBE:
                    //    break;
                    //case MessageType.SUBACK:
                    //    break;
                    //case MessageType.UNSUBSCRIBE:
                    //    break;
                    //case MessageType.UNSUBACK:
                    //    break;
                }
                OnMessageReceived?.Invoke(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ProcesMessage Error");
            }
        }

        public void Disconnect()
        {
            if (_conn.Recv != null)
                _conn.Recv -= ProcesMessage;
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
            _conn.SendMessage(msg);
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
            _conn.SendMessage(msg);
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
            _conn.SendMessage(msg);
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
            if (_conn != null)
            {
                Close();
                if (_conn != null)
                {
                    //connection.Dispose();
                }
            }
            if (_pingTimer != null)
                _pingTimer.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
