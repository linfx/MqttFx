MQTT协议是应用层协议，需要借助TCP/IP协议进行传输，类似HTTP协议。MQTT协议格式
[ Fixed Header | Variable Header | Payload]

# Fixed Header: 
固定头部，MQTT协议分很多种类型，如连接，发布，订阅，心跳等。其中固定头是必须的，所有类型的MQTT协议中，都必须包含固定头。

# Variable Header:
可变头部，可变头部不是可选的意思，而是指这部分在有些协议类型中存在，在有些协议中不存在。

# Payload:
消息载体，就是消息内容。与可变头一样，在有些协议类型中有消息内容，有些协议类型中没有消息内容