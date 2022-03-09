using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using System;

namespace MqttFx.Client.Channels
{
    /// <summary>
    /// 消息重发
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class RetransmissionHandler<T> where T : Packet
    {
        private volatile bool stopped;
        //private final PendingOperation pendingOperation;
        private IScheduledTask timer;
        private int timeout;
        private Action<T> handler;
        private readonly T originalMessage;

        public RetransmissionHandler(T originalMessage)
        {
            this.originalMessage = originalMessage;
        }

        public void Start(IEventLoop eventLoop)
        {
            if (eventLoop is null)
                throw new ArgumentNullException(nameof(eventLoop));

            timeout = 10;
            StartTimer(eventLoop);
        }

        void StartTimer(IEventLoop eventLoop)
        {
            //if (stopped || pendingOperation.isCanceled())
            //    return;

            if (stopped)
                return;

            timer = eventLoop.Schedule(() =>
            {
                //if (stopped || pendingOperation.isCanceled())
                //    return;

                if (stopped)
                    return;

                timeout += 5;

                handler(originalMessage);
                StartTimer(eventLoop);
            }, TimeSpan.FromSeconds(timeout));
        }

        public void Stop()
        {
            stopped = true;
            if (timer != null)
                timer.Cancel();
        }

        public void SetHandle(Action<T> handler)
        {
            this.handler = handler;
        }
    }
}
