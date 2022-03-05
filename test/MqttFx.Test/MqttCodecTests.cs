using DotNetty.Buffers;
using DotNetty.Codecs.MqttFx;
using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Transport.Channels;
using Moq;
using System.Collections.Generic;
using System.Linq;
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
        [InlineData("a", true, 0, null, null, "will/topic/name", new byte[] { 5, 3, 255, 6, 5 }, MqttQos.EXACTLY_ONCE, true)]
        [InlineData("11a_2", false, 1, "user1", null, "will", new byte[0], MqttQos.AT_LEAST_ONCE, false)]
        [InlineData("abc/ж", false, 10, "", "pwd", null, null, null, false)]
        [InlineData("", true, 1000, "имя", "密碼", null, null, null, false)]
        public void ConnectMessageTest(string clientId, bool cleanSession, ushort keepAlive, string userName, string password, string willTopicName, byte[] willMessage, MqttQos? willQos, bool willRetain)
        {
            var packet = new ConnectPacket();
            var packet_variableHeader = (ConnectVariableHeader)packet.VariableHeader;
            var packet_payload = (ConnectPayload)packet.Payload;

            packet_payload.ClientId = clientId;
            packet_variableHeader.ConnectFlags.CleanSession = cleanSession;
            packet_variableHeader.KeepAlive = keepAlive;
            if (userName != null)
            {
                packet_variableHeader.ConnectFlags.UsernameFlag = true;
                packet_payload.UserName = userName;
            }
            if (password != null)
            {
                packet_variableHeader.ConnectFlags.PasswordFlag = true;
                packet_payload.Password = password;
            }
            if (willTopicName != null)
            {
                packet_variableHeader.ConnectFlags.WillFlag = true;
                packet_variableHeader.ConnectFlags.WillQos = willQos ?? MqttQos.AT_MOST_ONCE;
                packet_variableHeader.ConnectFlags.WillRetain = willRetain;
                packet_payload.WillTopic = willTopicName;
                packet_payload.WillMessage = willMessage;
            }

            var recoded = RecodePacket(packet, true, false);
            var recoded_variableHeader = (ConnectVariableHeader)recoded.VariableHeader;
            var recoded_payload = (ConnectPayload)recoded.Payload;

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<ConnectPacket>()), Times.Once);
            Assert.Equal(packet_payload.ClientId, recoded_payload.ClientId);
            Assert.Equal(packet_variableHeader.KeepAlive, recoded_variableHeader.KeepAlive);
            Assert.Equal(packet_variableHeader.ConnectFlags.CleanSession, recoded_variableHeader.ConnectFlags.CleanSession);
            Assert.Equal(packet_variableHeader.ConnectFlags.UsernameFlag, recoded_variableHeader.ConnectFlags.UsernameFlag);
            if (packet_variableHeader.ConnectFlags.UsernameFlag)
            {
                Assert.Equal(recoded_payload.UserName, recoded_payload.UserName);
            }
            Assert.Equal(packet_variableHeader.ConnectFlags.PasswordFlag, packet_variableHeader.ConnectFlags.PasswordFlag);
            if (packet_variableHeader.ConnectFlags.PasswordFlag)
            {
                Assert.Equal(recoded_payload.Password, recoded_payload.Password);
            }
            if (packet_variableHeader.ConnectFlags.WillFlag)
            {
                Assert.Equal(packet_variableHeader.ConnectFlags.WillQos, recoded_variableHeader.ConnectFlags.WillQos);
                Assert.Equal(packet_variableHeader.ConnectFlags.WillRetain, recoded_variableHeader.ConnectFlags.WillRetain);
                Assert.Equal(recoded_payload.WillTopic, recoded_payload.WillTopic);
                //Assert.True(Equals(Unpooled.WrappedBuffer(packet_payload.WillMessage), recoded_payload.WillMessage));
            }
        }

        [Theory]
        [InlineData(false, ConnectReturnCode.CONNECTION_ACCEPTED)]
        [InlineData(true, ConnectReturnCode.CONNECTION_ACCEPTED)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_UNACCEPTABLE_PROTOCOL_VERSION)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_IDENTIFIER_REJECTED)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_SERVER_UNAVAILABLE)]
        [InlineData(false, ConnectReturnCode.CONNECTION_REFUSED_BAD_USER_NAME_OR_PASSWORD)]
        public void ConnAckMessageTest(bool sessionPresent, ConnectReturnCode returnCode)
        {
            var packet = new ConnAckPacket();
            var packet_variableHeader = (ConnAckVariableHeader)packet.VariableHeader;
            packet_variableHeader.SessionPresent = sessionPresent;
            packet_variableHeader.ConnectReturnCode = returnCode;

            var recoded = RecodePacket(packet, false, false);
            var recoded_variableHeader = (ConnAckVariableHeader)packet.VariableHeader;

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<ConnAckPacket>()), Times.Once);
            Assert.Equal(packet_variableHeader.SessionPresent, recoded_variableHeader.SessionPresent);
            Assert.Equal(packet_variableHeader.ConnectReturnCode, recoded_variableHeader.ConnectReturnCode);
        }

        [Theory]
        [InlineData(1, new[] { "+", "+/+", "//", "/#", "+//+" }, new[] { MqttQos.EXACTLY_ONCE, MqttQos.AT_LEAST_ONCE, MqttQos.AT_MOST_ONCE, MqttQos.EXACTLY_ONCE, MqttQos.AT_MOST_ONCE })]
        [InlineData(ushort.MaxValue, new[] { "a" }, new[] { MqttQos.AT_LEAST_ONCE })]
        public void SubscribeMessageTest(ushort packetId, string[] topicFilters, MqttQos[] requestedQosValues)
        {
            var packet = new SubscribePacket(packetId, topicFilters.Zip(requestedQosValues, (topic, qos) =>
            {
                SubscriptionRequest ts;
                ts.TopicFilter = topic;
                ts.RequestedQos = qos;
                return ts;
            }).ToArray());

            var recoded = RecodePacket(packet, true, true);

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<SubscribePacket>()), Times.Once);
            Assert.Equal(packet.SubscriptionRequests, recoded.SubscriptionRequests, EqualityComparer<SubscriptionRequest>.Default);
            Assert.Equal(packet.PacketId, recoded.PacketId);
        }

        [Theory]
        [InlineData(1, new[] { MqttQos.EXACTLY_ONCE, MqttQos.AT_LEAST_ONCE, MqttQos.AT_MOST_ONCE, MqttQos.FAILURE })]
        [InlineData(ushort.MaxValue, new[] { MqttQos.AT_LEAST_ONCE })]
        public void SubAckMessageTest(ushort packetId, MqttQos[] qosValues)
        {
            var packet = new SubAckPacket(packetId, qosValues);

            var recoded = RecodePacket(packet, false, true);

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<SubAckPacket>()), Times.Once);
            Assert.Equal(packet.ReturnCodes, recoded.ReturnCodes);
            Assert.Equal(packet.PacketId, recoded.PacketId);
        }

        [Theory]
        [InlineData(1, new[] { "+", "+/+", "//", "/#", "+//+" })]
        [InlineData(ushort.MaxValue, new[] { "a" })]
        public void UnsubscribeMessageTest(ushort packetId, string[] topicFilters)
        {
            var packet = new UnsubscribePacket(packetId, topicFilters);

            var recoded = RecodePacket(packet, true, true);

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<UnsubscribePacket>()), Times.Once);
            Assert.Equal(packet.TopicFilters, recoded.TopicFilters);
            Assert.Equal(packet.PacketId, recoded.PacketId);
        }

        [Theory]
        [InlineData(MqttQos.AT_MOST_ONCE, false, false, 1, "a", new byte[0])]
        [InlineData(MqttQos.EXACTLY_ONCE, true, false, ushort.MaxValue, "/", new byte[0])]
        [InlineData(MqttQos.AT_LEAST_ONCE, false, true, 129, "a/b", new byte[] { 1, 2, 3 })]
        [InlineData(MqttQos.EXACTLY_ONCE, true, true, ushort.MaxValue - 1, "topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/topic/name/that/is/longer/than/256/characters/", new byte[] { 1 })]
        public void PublishMessageTest(MqttQos qos, bool dup, bool retain, ushort packetId, string topicName, byte[] payload)
        {
            var packet = new PublishPacket(qos, dup, retain)
            {
                TopicName = topicName
            };
            var packet_payload = (PublishPayload)packet.Payload;

            if (qos > MqttQos.AT_MOST_ONCE)
            {
                packet.PacketId = packetId;
            }
            packet_payload.Data = payload;

            var recoded = RecodePacket(packet, false, true);
            var recoded_payload = (PublishPayload)recoded.Payload;

            contextMock.Verify(x => x.FireChannelRead(It.IsAny<PublishPacket>()), Times.Once);
            Assert.Equal(packet.TopicName, recoded.TopicName);
            if (packet.Qos > MqttQos.AT_MOST_ONCE)
            {
                Assert.Equal(packet.PacketId, recoded.PacketId);
            }
            Assert.Equal(packet_payload.Data, recoded_payload.Data);
        }

        //[Theory]
        //[InlineData(1)]
        ////[InlineData(127)]
        ////[InlineData(128)]
        ////[InlineData(256)]
        ////[InlineData(257)]
        ////[InlineData(ushort.MaxValue)]
        //public void PacketIdOnlyResponseMessagesTest(ushort packetId)
        //{
        //    //this.PublishResponseMessageTest<PubAckPacket>(packetId, true);
        //    //this.PublishResponseMessageTest<PubAckPacket>(packetId, false);
        //    //this.PublishResponseMessageTest<PubRecPacket>(packetId, true);
        //    //this.PublishResponseMessageTest<PubRecPacket>(packetId, false);
        //    //this.PublishResponseMessageTest<PubRelPacket>(packetId, true);
        //    //this.PublishResponseMessageTest<PubRelPacket>(packetId, false);
        //    //this.PublishResponseMessageTest<PubCompPacket>(packetId, true);
        //    //this.PublishResponseMessageTest<PubCompPacket>(packetId, false);
        //    PublishResponseMessageTest<UnsubAckPacket>(packetId, false);
        //}

        //void PublishResponseMessageTest<T>(ushort packetId, bool useServer)
        //    where T : PacketWithId, new()
        //{
        //    var packet = new T
        //    {
        //        PacketId = packetId
        //    };

        //    var recoded = RecodePacket(packet, useServer, true);

        //    contextMock.Verify(x => x.FireChannelRead(It.IsAny<T>()), Times.Once);
        //    contextMock.ResetCalls();
        //    Assert.Equal(packet.PacketId, recoded.PacketId);
        //}

        [Fact]
        public void EmptyPacketMessagesTest()
        {
            EmptyPacketMessageTest(PingReqPacket.Instance, true);
            EmptyPacketMessageTest(PingRespPacket.Instance, false);
            EmptyPacketMessageTest(DisconnectPacket.Instance, true);
        }

        private void EmptyPacketMessageTest<T>(T packet, bool useServer) where T : Packet
        {
            T recoded = RecodePacket(packet, useServer, false);
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
