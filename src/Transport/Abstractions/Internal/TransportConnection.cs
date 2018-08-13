using System;
using System.IO.Pipelines;
using System.Threading;

namespace nMqtt.Transport.Abstractions.Internal
{
    public abstract class TransportConnection : IDisposable
    {
        public IDuplexPipe Application { get; set; }

        public PipeWriter Input => Application.Output;

        public PipeReader Output => Application.Input;

        public CancellationToken ConnectionClosed { get; set; }

        public virtual void Dispose()
        {
        }
    }
}
