using DotNetty.Buffers;
using nMqtt.Packets;
using nMqtt.Protocol;
using System.Collections.Generic;
using Xunit;

namespace nMqtt.Test
{
    public class MqttCodecTests
    {
        static readonly IByteBufferAllocator Allocator = new UnpooledByteBufferAllocator();

        [Theory]
        //[InlineData("a", true, 0, null, null, "will/topic/name", new byte[] { 5, 3, 255, 6, 5 }, QualityOfService.ExactlyOnce, true)]
        //[InlineData("11a_2", false, 1, "user1", null, "will", new byte[0], QualityOfService.AtLeastOnce, false)]
        [InlineData("abc/ж", false, 10, "", "pwd", null, null, null, false)]
        [InlineData("", true, 1000, "имя", "密碼", null, null, null, false)]
        public void TestConnectMessage(string clientId, bool cleanSession, int keepAlive, string userName, string password, string willTopicName, byte[] willMessage, MqttQos qos, bool willRetain)
        {
            var packet = new ConnectPacket();
            packet.ClientId = clientId;
            packet.CleanSession = cleanSession;
            //packet.KeepAlive = keepAlive;
            if (userName != null)
            {
                packet.UserName = userName;
                if (password != null)
                {
                    packet.Password = password;
                }
            }
            if (willTopicName != null)
            {
                packet.WillTopic = willTopicName;
                //packet.WillMessage = Unpooled.WrappedBuffer(willMessage);
                //packet.WillQualityOfService = willQos ?? QualityOfService.AtMostOnce;
                packet.WillRetain = willRetain;
            }

            ConnectPacket recoded = this.RecodePacket(packet, true, true);

            //this.contextMock.Verify(x => x.FireChannelRead(It.IsAny<ConnectPacket>()), Times.Once);
            //Assert.Equal(packet.ClientId, recoded.ClientId);
            //Assert.Equal(packet.CleanSession, recoded.CleanSession);
            //Assert.Equal(packet.KeepAliveInSeconds, recoded.KeepAliveInSeconds);
            //Assert.Equal(packet.HasUsername, recoded.HasUsername);
            //if (packet.HasUsername)
            //{
            //    Assert.Equal(packet.Username, recoded.Username);
            //}
            //Assert.Equal(packet.HasPassword, recoded.HasPassword);
            //if (packet.HasPassword)
            //{
            //    Assert.Equal(packet.Password, recoded.Password);
            //}
            //if (packet.HasWill)
            //{
            //    Assert.Equal(packet.WillTopicName, recoded.WillTopicName);
            //    Assert.True(ByteBufferUtil.Equals(Unpooled.WrappedBuffer(willMessage), recoded.WillMessage));
            //    Assert.Equal(packet.WillQualityOfService, recoded.WillQualityOfService);
            //    Assert.Equal(packet.WillRetain, recoded.WillRetain);
            //}
        }


        [Theory]
        [InlineData(false, ConnectReturnCode.ConnectionAccepted)]
        [InlineData(true, ConnectReturnCode.ConnectionAccepted)]
        [InlineData(false, ConnectReturnCode.UnacceptedProtocolVersion)]
        //[InlineData(false, ConnectReturnCode.RefusedIdentifierRejected)]
        //[InlineData(false, ConnectReturnCode.RefusedServerUnavailable)]
        [InlineData(false, ConnectReturnCode.BadUsernameOrPassword)]
        [InlineData(false, ConnectReturnCode.RefusedNotAuthorized)]
        public void TestConnAckMessage(bool sessionPresent, ConnectReturnCode returnCode)
        {
            var packet = new ConnAckPacket
            {
                SessionPresent = sessionPresent,
                ConnectReturnCode = returnCode
            };

            //ConnAckPacket recoded = this.RecodePacket(packet, false, true);

            //this.contextMock.Verify(x => x.FireChannelRead(It.IsAny<ConnAckPacket>()), Times.Once);
            //Assert.Equal(packet.SessionPresent, recoded.SessionPresent);
            //Assert.Equal(packet.ReturnCode, recoded.ReturnCode);
        }

        T RecodePacket<T>(T packet, bool useServer, bool explodeForDecode) where T : Packet
        {
            var output = new List<object>();
            MqttEncoder.DoEncode(Allocator, packet, output);

            T observedPacket = null;
            //this.contextMock.Setup(x => x.FireChannelRead(It.IsAny<T>()))
            //    .Callback((object message) => observedPacket = Assert.IsAssignableFrom<T>(message))
            //    .Returns(this.contextMock.Object);

            //foreach (IByteBuffer message in output)
            //{
            //    MqttDecoder mqttDecoder = useServer ? this.serverDecoder : this.clientDecoder;
            //    if (explodeForDecode)
            //    {
            //        while (message.IsReadable())
            //        {
            //            IByteBuffer finalBuffer = message.ReadBytes(1);
            //            mqttDecoder.ChannelRead(this.contextMock.Object, finalBuffer);
            //        }
            //    }
            //    else
            //    {
            //        mqttDecoder.ChannelRead(this.contextMock.Object, message);
            //    }
            //}
            return observedPacket;
        }
    }
}
