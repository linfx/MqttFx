# MqttFx
```
c# mqtt 3.1.1 client
```

## Install

`PM> Install-Package MqttFx`


## Examples
```c#
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MqttFx;
using DotNetty.Codecs.MqttFx.Packets;

namespace Echo.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddMqttClient(options =>
            {
                options.Server = "118.126.96.166";
            });
            var container = services.BuildServiceProvider();

            var client = container.GetService<MqttClient>();
            client.OnConnected += Connected;
            client.OnDisconnected += Disconnected;
            client.OnMessageReceived += MessageReceived;
            if (await client.ConnectAsync() == ConnectReturnCode.ConnectionAccepted)
            {
                //var top = "/World";
                //Console.WriteLine("Subscribe:" + top);
                //Console.Write("SubscribeReturnCode: ");
                //var r = await client.SubscribeAsync(top, MqttQos.ExactlyOnce);
                //Console.WriteLine(r.ReturnCodes);
                while (true)
                {
                    await client.PublishAsync("/World", Encoding.UTF8.GetBytes("Hello World!"), MqttQos.AtLeastOnce);
                    await Task.Delay(2000);
                }
            }
            Console.ReadKey();
        }

        private static void Connected(ConnectReturnCode connectResponse)
        {
            Console.WriteLine("Connected Ssuccessful!, ConnectReturnCode: " + connectResponse);
        }

        private static void Disconnected()
        {
            Console.WriteLine("Disconnected");
        }

        private static void MessageReceived(Message message)
        {
            var result = Encoding.UTF8.GetString(message.Payload);
            Console.WriteLine(result);
        }
    }
}

```


## EMQ  百万级分布式开源物联网MQTT消息服务器
http://www.emqtt.com/

***************************

## 概览
#### MQTT是一个轻量的发布订阅模式消息传输协议，专门针对低带宽和不稳定网络环境的物联网应用设计
```
MQTT 官网:           http://mqtt.org
MQTT V3.1.1协议规范: http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/os/mqtt-v3.1.1-os.html
MQTT 协议英文版:     http://docs.oasis-open.org/mqtt/mqtt/v3.1.1/mqtt-v3.1.1.html
MQTT 协议中文版:      https://mcxiaoke.gitbooks.io/mqtt-cn/content/

```

## 特点
```
1.开放消息协议，简单易实现
2.发布订阅模式，一对多消息发布
3.基于TCP/IP网络连接
4.1字节固定报头，2字节心跳报文，报文结构紧凑
5.消息QoS支持，可靠传输保证
```

## 应用
#### MQTT协议广泛应用于物联网、移动互联网、智能硬件、车联网、电力能源等领域。
```
1.物联网M2M通信，物联网大数据采集
2.Android消息推送，WEB消息推送
3.移动即时消息，例如Facebook Messenger
4.智能硬件、智能家具、智能电器
5.车联网通信，电动车站桩采集
6.智慧城市、远程医疗、远程教育
7.电力、石油与能源等行业市场
```

## MQTT基于主题(Topic)消息路由
```
chat/room/1
sensor/10/temperature
sensor/+/temperature
$SYS/broker/metrics/packets/received
$SYS/broker/metrics/#
```

#### 主题(Topic)通过’/’分割层级，支持’+’, ‘#’通配符:
```
'+': 表示通配一个层级，例如a/+，匹配a/x, a/y
'#': 表示通配多个层级，例如a/#，匹配a/x, a/b/c/d
```

#### 订阅者与发布者之间通过主题路由消息进行通信，例如采用mosquitto命令行发布订阅消息:
```
mosquitto_sub -t a/b/+ -q 1
mosquitto_pub -t a/b/c -m hello -q 1
```

#### 注解
`订阅者可以订阅含通配符主题，但发布者不允许向含通配符主题发布消息。`

***************************

## MQTT V3.1.1协议报文
#### 报文结构
```
固定报头(Fixed header)
可变报头(Variable header)
报文有效载荷(Payload)
```

#### 固定报头
```
| Bit   |  7 |  6 |  5 |  4 |  3 |  2 |  1 |  0 |
| ----- |----|----|----|----|----|----|----|----|
|byte1  | MQTT Packet type  | Flags
|byte2… | Remaining Length
```


#### 报文类型
| 类型名称  | 类型值 | 报文说明 |
| -------- | ---- | ------------- |
|CONNECT	|1	  |发起连接       |
|CONNACK	|2	  |连接回执       |
|PUBLISH	|3	  |发布消息       |
|PUBACK	    |4	  |发布回执       |
|PUBREC	    |5	  |QoS2消息回执	  |
|PUBREL	    |6	  |QoS2消息释放	  |
|PUBCOMP	|7	  |QoS2消息完成	  |
|SUBSCRIBE  |8	  |订阅主题		  |
|SUBACK	    |9	  |订阅回执		  |
|UNSUBSCRIBE|10	  |取消订阅		  |
|UNSUBACK	|11	  |取消订阅回执	  |
|PINGREQ	|12	  |PING请求		  |
|PINGRESP	|13	  |PING响应		  |
|DISCONNECT	|14	  |断开连接		  |

### PUBLISH发布消息
PUBLISH报文承载客户端与服务器间双向的发布消息。 PUBACK报文用于接收端确认QoS1报文，PUBREC/PUBREL/PUBCOMP报文用于QoS2消息流程。

### PINGREQ/PINGRESP心跳
客户端在无报文发送时，按保活周期(KeepAlive)定时向服务端发送PINGREQ心跳报文，服务端响应PINGRESP报文。PINGREQ/PINGRESP报文均2个字节。

### MQTT消息QoS
MQTT发布消息QoS保证不是端到端的，是客户端与服务器之间的。订阅者收到MQTT消息的QoS级别，最终取决于发布消息的QoS和主题订阅的QoS。

### Qos0消息发布订阅
![Image text](http://www.emqtt.com/docs/v2/_images/qos0_seq.png)

### Qos1消息发布订阅
![Image text](http://www.emqtt.com/docs/v2/_images/qos1_seq.png)

### Qos2消息发布订阅
![Image text](http://www.emqtt.com/docs/v2/_images/qos2_seq.png)

### MQTT会话(Clean Session)
MQTT客户端向服务器发起CONNECT请求时，可以通过’Clean Session’标志设置会话。

‘Clean Session’设置为0，表示创建一个持久会话，在客户端断开连接时，会话仍然保持并保存离线消息，直到会话超时注销。

‘Clean Session’设置为1，表示创建一个新的临时会话，在客户端断开时，会话自动销毁。

### MQTT连接保活心跳
MQTT客户端向服务器发起CONNECT请求时，通过KeepAlive参数设置保活周期。

客户端在无报文发送时，按KeepAlive周期定时发送2字节的PINGREQ心跳报文，服务端收到PINGREQ报文后，回复2字节的PINGRESP报文。

服务端在1.5个心跳周期内，既没有收到客户端发布订阅报文，也没有收到PINGREQ心跳报文时，主动心跳超时断开客户端TCP连接。

### MQTT遗愿消息(Last Will)
MQTT客户端向服务器端CONNECT请求时，可以设置是否发送遗愿消息(Will Message)标志，和遗愿消息主题(Topic)与内容(Payload)。

MQTT客户端异常下线时(客户端断开前未向服务器发送DISCONNECT消息)，MQTT消息服务器会发布遗愿消息。

### MQTT保留消息(Retained Message)
MQTT客户端向服务器发布(PUBLISH)消息时，可以设置保留消息(Retained Message)标志。保留消息(Retained Message)会驻留在消息服务器，后来的订阅者订阅主题时仍可以接收该消息。