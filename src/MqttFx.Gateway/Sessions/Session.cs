using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace MqttFx.Gateway.Sessions
{
    public class Session
    {
        public Session(Socket client)
        {
            Client = client;
        }

        public Socket Client { get; set; }
    }
}
