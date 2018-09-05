using Microsoft.Extensions.Logging;
using nMqtt.Transport.Abstractions.Internal;
using System;
using System.IO;
using System.IO.Pipelines;
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
        private readonly ILogger _logger;
        private readonly SocketReceiver _receiver; 
        private readonly SocketSender _sender;
        private readonly CancellationTokenSource _connectionClosedTokenSource = new CancellationTokenSource();

        private volatile bool _aborted;
        private long _totalBytesWritten;

        internal SocketConnection(Socket socket, PipeScheduler scheduler)
        {
            _socket = socket;
            _receiver = new SocketReceiver(_socket, scheduler);
            _sender = new SocketSender(_socket, scheduler);
        }

        public async Task StartAsync()
        {
            var p = DuplexPipe.CreateConnectionPair(PipeOptions.Default, PipeOptions.Default);
            Application = p.Application;

            try
            {
                // Spawn send and receive logic
                var receiveTask = DoReceive();
                var sendTask = DoSend();

                // If the sending task completes then close the receive
                // We don't need to do this in the other direction because the kestrel
                // will trigger the output closing once the input is complete.
                if (await Task.WhenAny(receiveTask, sendTask) == sendTask)
                {
                    // Tell the reader it's being aborted
                    _socket.Dispose();
                }

                // Now wait for both to complete
                await receiveTask;
                //await sendTask;

                
                _receiver.Dispose();
                _sender.Dispose();
                ThreadPool.QueueUserWorkItem(state => ((SocketConnection)state).CancelConnectionClosedToken(), this);
            }
            catch (Exception ex)
            {
                _logger.LogError(0, ex, $"Unexpected exception in {nameof(SocketConnection)}.{nameof(StartAsync)}.");
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
                    break;
                }

                Input.Advance(bytesReceived);       //告诉PipeWriter从套邛字读取了多少

                var flushTask = Input.FlushAsync(); //标记数据可用, 让PipeReader读取
                if (!flushTask.IsCompleted)
                {
                    await flushTask;
                }

                var result = flushTask.GetAwaiter().GetResult();
                if (result.IsCompleted)
                {
                    // Pipe consumer is shut down, do we stop writing
                    break;
                }
            }
        }


        private async Task<Exception> DoSend()
        {
            Exception error = null;

            try
            {
                await ProcessSends();
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (IOException ex)
            {
                error = ex;
            }
            catch (Exception ex)
            {
                error = new IOException(ex.Message, ex);
            }
            finally
            {
                _aborted = true;
                _socket.Shutdown(SocketShutdown.Both);
            }

            return error;
        }

        private async Task ProcessSends()
        {
            while (true)
            {
                // Wait for data to write from the pipe producer
                var result = await Output.ReadAsync();
                var buffer = result.Buffer;

                if (result.IsCanceled)
                {
                    break;
                }

                var end = buffer.End;
                var isCompleted = result.IsCompleted;
                if (!buffer.IsEmpty)
                {
                    await _sender.SendAsync(buffer);
                }

                Output.AdvanceTo(end);

                if (isCompleted)
                {
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