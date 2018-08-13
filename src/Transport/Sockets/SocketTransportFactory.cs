using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace nMqtt.Transport.Sockets
{
    public sealed class SocketTransportFactory
    {
        private readonly SocketTransportOptions _options;
        private readonly ILogger _logger;

        public SocketTransportFactory(
            IOptions<SocketTransportOptions> options,
            ILoggerFactory loggerFactory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _options = options.Value;
            _logger = loggerFactory.CreateLogger<SocketTransportFactory>();
        }

        public SocketTransport Create()
        {
            return new SocketTransport(_logger, _options.IOQueueCount, _options.MemoryPoolFactory());
        }
    }
}
