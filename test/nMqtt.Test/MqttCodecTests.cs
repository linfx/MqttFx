using System.Collections.Generic;
using System.Linq;
using DotNetty.Buffers;
using DotNetty.Codecs.MqttFx;
using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using Moq;
using Xunit;

namespace MqttFx.Test
{
    public class MqttCodecTests
    {
        private static readonly IByteBufferAllocator Allocator = new UnpooledByteBufferAllocator();

        private readonly MqttDecoder serverDecoder;
        private readonly MqttDecoder clientDecoder;
        private readonly Mock<IChannelHandlerContext> contextMock;

        public MqttCodecTests()
        {
            serverDecoder = new MqttDecoder(true, 256 * 1024);
            clientDecoder = new MqttDecoder(false, 256 * 1024);
            contextMock = new Mock<IChannelHandlerContext>(MockBehavior.Strict);
            contextMock.Setup(x => x.Removed).Returns(false);
            contextMock.Setup(x => x.Allocator).Returns(UnpooledByteBufferAllocator.Default);
        }

        [Theory]
        [InlineData("a", true, 0, null, null, "will/topic/name", new byte[] { 5, 3, 255, 6, 5 }, MqttQos.ExactlyOnce, true)]
        [InlineData("11a_2", false, 1, "user1", null, "will", new byte[0], MqttQos.AtLeastOnce, false)]
        [InlineData("abc/ж", false, 10, "", "pwd", null, null, null, false)]
        [InlineData("", true, 1000, "имя", "密碼", null, null, null, false)]
        public void TestConnectMessage(string clientId, bool cleanSession, ushort keepAlive, string userName, string password, string willTopicName, byte[] willMessage, MqttQos? willQos, bool willRetain)
        {
            var packet = new ConnectPacket
            {
                ClientId = clientId,
                CleanSession = cleanSession,
                KeepAlive = keepAlive
            };
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
                packet.WillMessage = willMessage;
                packet.WillQos = willQos ?? MqttQos.AtMostOnce;
                packet.WillRetain = willRetain;
            }

            ConnectPacket recoded = RecodePacket(packet, true, true);

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<ConnectPacket>()), Times.Once);
            Assert.Equal(packet.ClientId, recoded.ClientId);
            Assert.Equal(packet.CleanSession, recoded.CleanSession);
            Assert.Equal(packet.KeepAlive, recoded.KeepAlive);
            Assert.Equal(packet.UsernameFlag, recoded.UsernameFlag);
            if (packet.UsernameFlag)
            {
                Assert.Equal(packet.UserName, recoded.UserName);
            }
            Assert.Equal(packet.PasswordFlag, recoded.PasswordFlag);
            if (packet.PasswordFlag)
            {
                Assert.Equal(packet.Password, recoded.Password);
            }
            if (packet.WillFlag)
            {
                Assert.Equal(packet.WillTopic, recoded.WillTopic);
                //Assert.True(ByteBufferUtil.Equals(Unpooled.WrappedBuffer(willMessage), recoded.WillMessage));
                Assert.Equal(packet.Qos, recoded.Qos);
                Assert.Equal(packet.WillRetain, recoded.WillRetain);
            }
        }

        [Theory]
        [InlineData(false, ConnectReturnCode.CONNECTION_ACCEPTED)]
        [InlineData(true, ConnectReturnCode.CONNECTION_ACCEPTED)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_UNACCEPTABLE_PROTOCOL_VERSION)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_IDENTIFIER_REJECTED)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_SERVER_UNAVAILABLE)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_BAD_USER_NAME_OR_PASSWORD)]
        public void TestConnAckMessage(bool sessionPresent, ConnectReturnCode returnCode)
        {
            var packet = new ConnAckPacket();
            packet.VariableHeader.SessionPresent = sessionPresent;
            packet.VariableHeader.ConnectReturnCode = returnCode;

            var recoded = RecodePacket(packet, false, true);

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<ConnAckPacket>()), Times.Once);
            Assert.Equal(packet.VariableHeader.SessionPresent, recoded.VariableHeader.SessionPresent);
            Assert.Equal(packet.VariableHeader.ConnectReturnCode, recoded.VariableHeader.ConnectReturnCode);
        }

        [Theory]
        [InlineData(1, new[] { "+", "+/+", "//", "/#", "+//+" }, new[] { MqttQos.ExactlyOnce, MqttQos.AtLeastOnce, MqttQos.AtMostOnce, MqttQos.ExactlyOnce, MqttQos.AtMostOnce })]
        [InlineData(ushort.MaxValue, new[] { "a" }, new[] { MqttQos.AtLeastOnce })]
        public void TestSubscribeMessage(int packetId, string[] topicFilters, MqttQos[] requestedQosValues)
        {
            //var packet = new SubscribePacket(packetId, topicFilters.Zip(requestedQosValues, (topic, qos) => new SubscriptionRequest(topic, qos)).ToArray());

            var packet = new SubscribePacket();
            //packet.Add(topicFilters.Zip(requestedQosValues, (topic, qos) => new ))

            SubscribePacket recoded = RecodePacket(packet, true, true);

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<SubscribePacket>()), Times.Once);
            //Assert.Equal(packet.Requests, recoded.Requests, EqualityComparer<SubscriptionRequest>.Default);
            //Assert.Equal(packet.PacketId, recoded.PacketId);
        }

        [Theory]
        [InlineData(1, new[] { MqttQos.ExactlyOnce, MqttQos.AtLeastOnce, MqttQos.AtMostOnce, MqttQos.Failure })]
        [InlineData(ushort.MaxValue, new[] { MqttQos.AtLeastOnce })]
        public void TestSubAckMessage(ushort packetId, MqttQos[] qosValues)
        {
            var packet = new SubAckPacket
            {
                PacketId = packetId,
                ReturnCodes = qosValues,
            };

            SubAckPacket recoded = RecodePacket(packet, false, true);

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<SubAckPacket>()), Times.Once);
            Assert.Equal(packet.ReturnCodes, recoded.ReturnCodes);
            Assert.Equal(packet.PacketId, recoded.PacketId);
        }

        [Theory]
        [InlineData(1, new[] { "+", "+/+", "//", "/#", "+//+" })]
        [InlineData(ushort.MaxValue, new[] { "a" })]
        public void TestUnsubscribeMessage(ushort packetId, string[] topicFilters)
        {
            var packet = new UnsubscribePacket
            {
                PacketId = packetId,
            };
            packet.AddRange(topicFilters);

            UnsubscribePacket recoded = RecodePacket(packet, true, true);

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<UnsubscribePacket>()), Times.Once);
            //Assert.Equal(packet.TopicFilters, recoded.TopicFilters);
            Assert.Equal(packet.PacketId, recoded.PacketId);
        }

        [Theory]
        [InlineData(MqttQos.AtMostOnce, false, false, 1, "a", null)]
        [InlineData(MqttQos.ExactlyOnce, true, false, ushort.MaxValue, "/", new byte[0])]
        [InlineData(MqttQos.AtLeastOnce, false, true, 129, "a/b", new byte[] { 1, 2, 3 })]
        [InlineData(MqttQos.ExactlyOnce, true, true, ushort.MaxValue - 1, "topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/", new byte[] { 1 })]
        public void TestPublishMessage(MqttQos qos, bool dup, bool retain, ushort packetId, string topicName, byte[] payload)
        {
            var packet = new PublishPacket(qos, dup, retain)
            {
                TopicName = topicName
            };
            if (qos > MqttQos.AtMostOnce)
            {
                packet.PacketId = packetId;
            }
            packet.Payload = payload;

            PublishPacket recoded = RecodePacket(packet, false, true);

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<PublishPacket>()), Times.Once);
            Assert.Equal(packet.TopicName, recoded.TopicName);
            if (packet.Qos > MqttQos.AtMostOnce)
            {
                Assert.Equal(packet.PacketId, recoded.PacketId);
            }
            Assert.Equal(payload, recoded.Payload);
        }

        [Fact]
        public void TestEmptyPacketMessages()
        {
            TestEmptyPacketMessage(PingReqPacket.Instance, true);
            TestEmptyPacketMessage(PingRespPacket.Instance, false);
            TestEmptyPacketMessage(DisconnectPacket.Instance, true);
        }

        private void TestEmptyPacketMessage<T>(T packet, bool useServer) where T : Packet
        {
            T recoded = RecodePacket(packet, useServer, true);

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<T>()), Times.Once);
        }


        private T RecodePacket<T>(T packet, bool useServer, bool explodeForDecode) where T : Packet
        {
            var output = new List<object>();
            MqttEncoder.DoEncode(Allocator, packet, output);

            T observedPacket = null;
            contextMock.Setup(x => x.FireChannelRead(It.IsAny<T>()))
                .Callback((object message) => observedPacket = Assert.IsAssignableFrom<T>(message))
                .Returns(contextMock.Object);

            foreach (IByteBuffer message in output)
            {
                MqttDecoder mqttDecoder = useServer ? serverDecoder : clientDecoder;
                if (explodeForDecode)
                {
                    while (message.IsReadable())
                    {
                        IByteBuffer finalBuffer = message.ReadBytes(1);
                        mqttDecoder.ChannelRead(contextMock.Object, finalBuffer);
                    }
                }
                else
                {
                    mqttDecoder.ChannelRead(contextMock.Object, message);
                }
            }
            return observedPacket;
        }
    }
}
