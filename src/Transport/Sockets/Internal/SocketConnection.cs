using Microsoft.Extensions.Logging;
using nMqtt.Transport.Abstractions.Internal;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace nMqtt.Transport.Sockets.Internal
{
    internal sealed class SocketConnection : TransportConnection
    {
        public static readonly int MinimumSegmentSize = 4096;
        private static readonly int MinAllocBufferSize = MinimumSegmentSize / 2;
        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private readonly Socket _socket;
        private readonly PipeScheduler _scheduler;
        private readonly ILogger _trace;
        private readonly SocketReceiver _receiver;
        //private readonly SocketSender _sender;
        private readonly CancellationTokenSource _connectionClosedTokenSource = new CancellationTokenSource();

        private volatile bool _aborted;
        private long _totalBytesWritten;

        internal SocketConnection(Socket socket, MemoryPool<byte> memoryPool, PipeScheduler scheduler, ILogger logger)
        {
            Debug.Assert(socket != null);
            Debug.Assert(memoryPool != null);

            _socket = socket;
            MemoryPool = memoryPool;
            _scheduler = scheduler;
            _trace = logger;

            var localEndPoint = (IPEndPoint)_socket.LocalEndPoint;
            var remoteEndPoint = (IPEndPoint)_socket.RemoteEndPoint;

            // On *nix platforms, Sockets already dispatches to the ThreadPool.
            // Yes, the IOQueues are still used for the PipeSchedulers. This is intentional.
            // https://github.com/aspnet/KestrelHttpServer/issues/2573
            var awaiterScheduler = IsWindows ? _scheduler : PipeScheduler.Inline;

            _receiver = new SocketReceiver(_socket, awaiterScheduler);
            //_sender = new SocketSender(_socket, awaiterScheduler);
        }

        public MemoryPool<byte> MemoryPool { get; }
        public PipeScheduler InputWriterScheduler => _scheduler;
        public PipeScheduler OutputReaderScheduler => _scheduler;
        //public override long TotalBytesWritten => Interlocked.Read(ref _totalBytesWritten);

        public async Task StartAsync()
        {
            try
            {
                // Spawn send and receive logic
                var receiveTask = DoReceive();
                //var sendTask = DoSend();

                // Now wait for both to complete
                await receiveTask;
                //await sendTask;

                _receiver.Dispose();
                //_sender.Dispose();
                //ThreadPool.QueueUserWorkItem(state => ((SocketConnection)state).CancelConnectionClosedToken(), this);
            }
            catch (Exception ex)
            {
                _trace.LogError(0, ex, $"Unexpected exception in {nameof(SocketConnection)}.{nameof(StartAsync)}.");
            }
        }

        private async Task DoReceive()
        {
            Exception error = null;

            try
            {
                await ProcessReceives();
            }
            //catch (SocketException ex) when (IsConnectionResetError(ex.SocketErrorCode))
            //{
            //    // A connection reset can be reported as SocketError.ConnectionAborted on Windows
            //    if (!_aborted)
            //    {
            //        error = new ConnectionResetException(ex.Message, ex);
            //        _trace.ConnectionReset(ConnectionId);
            //    }
            //}
            //catch (SocketException ex) when (IsConnectionAbortError(ex.SocketErrorCode))
            //{
            //    if (!_aborted)
            //    {
            //        // Calling Dispose after ReceiveAsync can cause an "InvalidArgument" error on *nix.
            //        _trace.ConnectionError(ConnectionId, error);
            //    }
            //}
            //catch (ObjectDisposedException)
            //{
            //    if (!_aborted)
            //    {
            //        _trace.ConnectionError(ConnectionId, error);
            //    }
            //}
            catch (IOException ex)
            {
                error = ex;
                //_trace.ConnectionError(ConnectionId, error);
            }
            catch (Exception ex)
            {
                error = new IOException(ex.Message, ex);
                //_trace.ConnectionError(ConnectionId, error);
            }
            finally
            {
                //if (_aborted)
                //{
                //    error = error ?? _abortReason ?? new ConnectionAbortedException();
                //}

                Input.Complete(error);
            }
        }

        private async Task ProcessReceives()
        {
            while (true)
            {
                var buffer = Input.GetMemory(MinAllocBufferSize);  //从PipeWriter至少分配512字节
                var bytesReceived = await _receiver.ReceiveAsync(buffer);
                if (bytesReceived == 0)
                {
                    // FIN
                    //_trace.ConnectionReadFin(ConnectionId);
                    break;
                }
                Input.Advance(bytesReceived);       //告诉PipeWriter从套邛字读取了多少

                var flushTask = Input.FlushAsync(); //标记数据可用, 让PipeReader读取
                if (!flushTask.IsCompleted)
                {
                    //_trace.ConnectionPause(ConnectionId);
                    await flushTask;
                    //_trace.ConnectionResume(ConnectionId);
                }

                var result = flushTask.GetAwaiter().GetResult();
                if (result.IsCompleted)
                {
                    // Pipe consumer is shut down, do we stop writing
                    break;
                }
            }
        }

        private void CancelConnectionClosedToken()
        {
            try
            {
                _connectionClosedTokenSource.Cancel();
            }
            catch (Exception)
            {
                //_trace.LogError(0, ex, $"Unexpected exception in {nameof(SocketConnection)}.{nameof(CancelConnectionClosedToken)}.");
            }
        }
    }
}