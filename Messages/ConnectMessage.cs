using System;
using System.IO;

namespace nMqtt.Messages
{
    internal sealed class ConnectMessage : MqttMessage
    {
        //variable header
        public string ProtocolName { get; set; }
        public byte ProtocolVersion { get; set; }

        #region ConnectFlags

        public bool UsernameFlag { get; set; }
        public bool PasswordFlag { get; set; }
        public bool WillRetain { get; set; }
        public Qos WillQos { get; set; }
        public bool WillFlag { get; set; }
        public bool CleanSession { get; set; }

        #endregion

        public short KeepAlive { get; set; }

        //payload
        public string ClientId { get; set; }
        public string WillTopic { get; set; }
        public string WillMessage { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } 

        public ConnectMessage()
            : base(MessageType.CONNECT)
        {
            ProtocolName = "MQTT";
            ProtocolVersion = 0x04;
        }

        public override void Encode(Stream stream)
        {
            using (var body = new MemoryStream())
            {
                //variable header
                body.WriteString(ProtocolName);       //byte 1 - 8
                body.WriteByte(ProtocolVersion);      //byte 9

                //ConnectFlags.WriteTo(body);         //byte 10
                var flags = UsernameFlag.ToByte() << 7;
                flags |= PasswordFlag.ToByte() << 6;
                flags |= WillRetain.ToByte() << 5;
                flags |= ((byte)WillQos) << 3;
                flags |= WillFlag.ToByte() << 2;
                flags |=  CleanSession.ToByte() << 1;
                body.WriteByte((byte)flags);

                body.WriteShort(KeepAlive);      //byte 11 - 12

                //payload
                body.WriteString(ClientId);
                if (WillFlag)
                {
                    body.WriteString(WillTopic);
                    body.WriteString(WillMessage);
                }
                if (UsernameFlag)
                    body.WriteString(Username);
                if (PasswordFlag)
                    body.WriteString(Password);

                FixedHeader.RemaingLength = (int)body.Length;
                FixedHeader.WriteTo(stream);
                body.WriteTo(stream);
            }
        }
    }

    internal sealed class ConnAckMessage : MqttMessage
    {
        public bool SessionPresent { get; set; }
        public MqttConnectReturnCode ReturnCode { get; set; }

        public ConnAckMessage()
            : base(MessageType.CONNACK)
        {
        }

        public override void Decode(Stream stream)
        {
            SessionPresent = (stream.ReadByte() & 0x01) == 1;
            ReturnCode = (MqttConnectReturnCode)stream.ReadByte();
        }
    }

    [Flags]
    internal enum MqttConnectReturnCode : byte
    {
        ConnectionAccepted        = 0x00,
        UnacceptedProtocolVersion = 0x01,
        IdentifierRejected        = 0x02,
        BrokerUnavailable         = 0x03,
        BadUsernameOrPassword     = 0x04,
        NotAuthorized             = 0x05
    }
}
