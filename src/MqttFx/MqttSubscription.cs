using System;
using System.Collections.Generic;
using System.Text;

namespace MqttFx
{
    public class MqttSubscription
    {
        public string topic;
        public IMessageReceivedHandler handler;
    }
}
