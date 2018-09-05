using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace nMqtt.Extensions.Rpc
{
    public class MqttRpcClient : IDisposable
    {
        private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> _waitingCalls = new ConcurrentDictionary<string, TaskCompletionSource<byte[]>>();


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
