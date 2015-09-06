using System;
using System.Threading;
using System.Diagnostics;
using nMqtt.Messages;

namespace nMqtt
{
    public sealed class MqttClient : IDisposable
    {
        Timer pingTimer;
        MqttConnection conn;
        readonly AutoResetEvent connectionResetEvent;
        public Action<string, byte[]> MessageReceived;

        public MqttClient(string server, string clientId)
        {
            Server = server;
            ClientId = clientId;
            conn = new MqttConnection();
            connectionResetEvent = new AutoResetEvent(false);
        }

        public ConnectionState Connect()
        {
            return Connect(string.Empty, string.Empty);
        }

        public ConnectionState Connect(string username, string password = "")
        {
            conn.Connect(Server, Port);
            conn.Recv += DecodeMessage;

            var msg = new ConnectMessage();
            msg.ClientId = ClientId;
            msg.CleanSession = CleanSession;
            if (!string.IsNullOrEmpty(username))
            {
                msg.UsernameFlag = true;
                msg.Username = username;
            }
            if(!string.IsNullOrEmpty(password))
            {
                msg.PasswordFlag = true;
                msg.Password = password;
            }
            msg.KeepAlive = KeepAlive;
            conn.SendMessage(msg);

            if (!connectionResetEvent.WaitOne(5000, false))
            {
                ConnectionState = ConnectionState.Disconnecting;
                Dispose();
                ConnectionState = ConnectionState.Disconnected;
                return ConnectionState;
            }

            pingTimer = new Timer((state) =>
            {
                Ping();
            }, null, KeepAlive * 1000, KeepAlive * 1000);

            return ConnectionState;
        }

        public void Publish(string topic, byte[] data, Qos qos = Qos.AtMostOnce)
        {
            var msg = new PublishMessage();
            msg.FixedHeader.Qos = qos;
            //msg.MessageIdentifier += MessageIdentifier;
            msg.TopicName = topic;
            msg.Payload = data;
            conn.SendMessage(msg);
        }

        public void Subscribe(string topic, Qos qos = Qos.AtMostOnce)
        {
            var msg = new SubscribeMessage();
            msg.FixedHeader.Qos = Qos.AtLeastOnce;
            msg.MessageIdentifier = 0;
            msg.Subscribe(topic, qos);
            conn.SendMessage(msg);
        }

        public void Unsubscribe(string topic)
        {
            var msg = new UnsubscribeMessage();
            msg.FixedHeader.Qos = Qos.AtLeastOnce;
            msg.Unsubscribe(topic);
            conn.SendMessage(msg);
        }

        public string ClientId { get; set; }

        public string Server { get; set; } = "localhost";

        public int Port { get; set; } = 1883;

        public short KeepAlive { get; set; } = 60;

        public bool CleanSession { get; set; } = true;

        public ConnectionState ConnectionState { get; private set; }

        void DecodeMessage(byte[] buffer)
        {
            var msg = MqttMessage.DecodeMessage(buffer);
            Debug.WriteLine("onRecv:{0}", msg.FixedHeader.MessageType);
            switch (msg.FixedHeader.MessageType)
            {
                case MessageType.CONNECT:
                    break;
                case MessageType.CONNACK:
                    var connAckMsg = (ConnAckMessage)msg;
                    if (connAckMsg.ReturnCode == MqttConnectReturnCode.BrokerUnavailable ||
                       connAckMsg.ReturnCode == MqttConnectReturnCode.IdentifierRejected ||
                       connAckMsg.ReturnCode == MqttConnectReturnCode.UnacceptedProtocolVersion ||
                       connAckMsg.ReturnCode == MqttConnectReturnCode.NotAuthorized ||
                       connAckMsg.ReturnCode == MqttConnectReturnCode.BadUsernameOrPassword)
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
                    connectionResetEvent.Set();
                    break;
                case MessageType.PUBLISH:
                    var pubMsg = (PublishMessage)msg;
                    string topic = pubMsg.TopicName;
                    var data = pubMsg.Payload;
                    if(pubMsg.FixedHeader.Qos == Qos.AtLeastOnce)
                    {
                        var ackMsg = new PublishAckMessage();
                        ackMsg.MessageIdentifier = pubMsg.MessageIdentifier;
                        conn.SendMessage(ackMsg);
                    }
                    OnMessageReceived(topic, data);
                    break;
                case MessageType.PUBACK:
                    //var pubAckMsg = (MqttPublishAckMessage)msg;
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
                    //var subAckMsg = (SubscribeAckMessage)msg;
                    break;
                case MessageType.UNSUBSCRIBE:
                    break;
                case MessageType.UNSUBACK:
                    break;
                case MessageType.PINGREQ:
                    //var pingReqMsg = (MqttPingReqMessage)msg;
                    //var pingRespMsg = new MqttPingRespMessage();
                    //connection.SendMessage(pingRespMsg);
                    //pingTimer.Change(keepAlivePeriod, keepAlivePeriod);
                    break;
                case MessageType.PINGRESP:
                    //var pingRespMsg = (PingRespMessage)msg;
                    break;
                case MessageType.DISCONNECT:
                    Disconnect();
                    break;
                default:
                    break;
            }
        }

        void OnMessageReceived(string topic, byte[] data)
        {
            if (MessageReceived != null)
                MessageReceived(topic, data);
        }

        void Ping()
        {
            var pingMsg = new PingReqMessage();
            conn.SendMessage(pingMsg);
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
