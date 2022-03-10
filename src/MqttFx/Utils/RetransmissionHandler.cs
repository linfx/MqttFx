using DotNetty.Codecs.MqttFx.Packets;
using DotNetty.Common.Concurrency;
using DotNetty.Transport.Channels;
using System;

namespace MqttFx.Utils;

/// <summary>
/// 消息重发
/// </summary>
/// <typeparam name="T"></typeparam>
class RetransmissionHandler<T> where T : Packet
{
    private volatile bool stopped;
    //private PendingOperation pendingOperation;
    private IScheduledTask timer;
    private int timeout;
    public Action<T> Handler { get; set; }
    public T OriginalMessage { get; set; }

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

            Handler(OriginalMessage);
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
        this.Handler = handler;
    }
}
