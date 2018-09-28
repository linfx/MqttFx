using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;

namespace MqttFx
{
    public sealed class ReadListeningHandler : ChannelHandlerAdapter
    {
        readonly Queue<object> _receivedQueue = new Queue<object>();
        readonly Queue<TaskCompletionSource<object>> _readPromises = new Queue<TaskCompletionSource<object>>();
        readonly TimeSpan _defaultReadTimeout;
        readonly object _gate = new object();

        volatile Exception registeredException;

        public ReadListeningHandler()
            : this(TimeSpan.FromSeconds(30))
        {
        }

        public ReadListeningHandler(TimeSpan defaultReadTimeout)
        {
            _defaultReadTimeout = defaultReadTimeout;
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            lock (_gate)
            {
                if (_readPromises.Count > 0)
                {
                    var promise = _readPromises.Dequeue();
                    promise.TrySetResult(message);
                }
                else
                {
                    _receivedQueue.Enqueue(message);
                }
            }
        }

        public override void ChannelInactive(IChannelHandlerContext context)
        {
            SetException(new InvalidOperationException("Channel is closed."));
            base.ChannelInactive(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception) => SetException(exception);

        void SetException(Exception exception)
        {
            registeredException = exception;
            lock (_gate)
            {
                while (_readPromises.Count > 0)
                {
                    var promise = _readPromises.Dequeue();
                    promise.TrySetException(exception);
                }
            }
        }

        public async Task<object> ReceiveAsync(TimeSpan timeout = default)
        {
            if (registeredException != null)
                throw registeredException;

            var promise = new TaskCompletionSource<object>();
            lock (_gate)
            {
                if (_receivedQueue.Count > 0)
                    return _receivedQueue.Dequeue();

                _readPromises.Enqueue(promise);
            }

            timeout = timeout <= TimeSpan.Zero ? _defaultReadTimeout : timeout;
            if (timeout > TimeSpan.Zero)
            {
                var task = await Task.WhenAny(promise.Task, Task.Delay(timeout));

                if (task != promise.Task)
                    throw new TimeoutException("ReceiveAsync timed out");

                return promise.Task.Result;
            }

            return await promise.Task;
        }
    }
}
