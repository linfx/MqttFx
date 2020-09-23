//using MqttFx.Gateway.Sessions;
//using System;
//using System.Buffers;
//using System.Collections.Generic;
//using System.IO.Pipelines;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace MqttFx.Gateway
//{
//    class MqttTcpServer
//    {
//        private Socket server;

//        public Task StartAsync(CancellationToken cancellationToken)
//        {
//            //Logger.LogInformation($"JT808 TCP Server start at {IPAddress.Any}:{Configuration.TcpPort}.");
//            Task.Factory.StartNew(async () =>
//            {
//                while (!cancellationToken.IsCancellationRequested)
//                {
//                    var socket = await server.AcceptAsync();
//                    var Session = new Session(socket);
//                    //SessionManager.TryAdd(Session);
//                    await Task.Factory.StartNew(async (state) =>
//                    {
//                        var session = (Session)state;
//                        //if (Logger.IsEnabled(LogLevel.Information))
//                        //{
//                        //    Logger.LogInformation($"[Connected]:{session.Client.RemoteEndPoint}");
//                        //}
//                        var pipe = new Pipe();
//                        Task writing = FillPipeAsync(session, pipe.Writer);
//                        Task reading = ReadPipeAsync(session, pipe.Reader);
//                        await Task.WhenAll(reading, writing);
//                        //SessionManager.RemoveBySessionId(session.SessionID);
//                    }, Session);
//                }
//            }, cancellationToken);
//            return Task.CompletedTask;
//        }

//        private async Task FillPipeAsync(Session session, PipeWriter writer)
//        {
//            while (true)
//            {
//                try
//                {
//                    Memory<byte> memory = writer.GetMemory(Configuration.MiniNumBufferSize);
//                    //设备多久没发数据就断开连接 Receive Timeout.
//                    int bytesRead = await session.Client.ReceiveAsync(memory, SocketFlags.None, session.ReceiveTimeout.Token);
//                    if (bytesRead == 0)
//                    {
//                        break;
//                    }
//                    writer.Advance(bytesRead);
//                }
//                catch (OperationCanceledException ex)
//                {
//                    Logger.LogError($"[Receive Timeout]:{session.Client.RemoteEndPoint}");
//                    break;
//                }
//                catch (System.Net.Sockets.SocketException ex)
//                {
//                    Logger.LogError($"[{ex.SocketErrorCode.ToString()},{ex.Message}]:{session.Client.RemoteEndPoint}");
//                    break;
//                }
//#pragma warning disable CA1031 // Do not catch general exception types
//                catch (Exception ex)
//                {
//                    Logger.LogError(ex, $"[Receive Error]:{session.Client.RemoteEndPoint}");
//                    break;
//                }
//#pragma warning restore CA1031 // Do not catch general exception types
//                FlushResult result = await writer.FlushAsync();
//                if (result.IsCompleted)
//                {
//                    break;
//                }
//            }
//            writer.Complete();
//        }
//        private async Task ReadPipeAsync(Session session, PipeReader reader)
//        {
//            while (true)
//            {
//                ReadResult result = await reader.ReadAsync();
//                if (result.IsCompleted)
//                {
//                    break;
//                }
//                ReadOnlySequence<byte> buffer = result.Buffer;
//                SequencePosition consumed = buffer.Start;
//                SequencePosition examined = buffer.End;
//                try
//                {
//                    if (result.IsCanceled) break;
//                    if (buffer.Length > 0)
//                    {
//                        ReaderBuffer(ref buffer, session, out consumed, out examined);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    //Logger.LogError(ex, $"[ReadPipe Error]:{session.Client.RemoteEndPoint}");
//                    break;
//                }
//                finally
//                {
//                    reader.AdvanceTo(consumed, examined);
//                }
//            }
//            reader.Complete();
//        }

//        private void ReaderBuffer(ref ReadOnlySequence<byte> buffer, Session session, out SequencePosition consumed, out SequencePosition examined)
//        {
//            consumed = buffer.Start;
//            examined = buffer.End;
//            //SequenceReader<byte> seqReader = new SequenceReader<byte>(buffer);
//            //if (seqReader.TryPeek(out byte beginMark))
//            //{
//            //    if (beginMark != JT808Package.BeginFlag) throw new ArgumentException("Not JT808 Packages.");
//            //}
//            //byte mark = 0;
//            //long totalConsumed = 0;
//            //while (!seqReader.End)
//            //{
//            //    if (seqReader.IsNext(JT808Package.BeginFlag, advancePast: true))
//            //    {
//            //        if (mark == 1)
//            //        {
//            //            ReadOnlySpan<byte> contentSpan = ReadOnlySpan<byte>.Empty;
//            //            try
//            //            {
//            //                contentSpan = seqReader.Sequence.Slice(totalConsumed, seqReader.Consumed - totalConsumed).FirstSpan;
//            //                //过滤掉不是808标准包（14）
//            //                //（头）1+（消息 ID ）2+（消息体属性）2+（终端手机号）6+（消息流水号）2+（检验码 ）1+（尾）1
//            //                if (contentSpan.Length > 14)
//            //                {
//            //                    var package = Serializer.HeaderDeserialize(contentSpan, minBufferSize: 10240);
//            //                    AtomicCounterService.MsgSuccessIncrement();
//            //                    if (Logger.IsEnabled(LogLevel.Debug)) Logger.LogDebug($"[Atomic Success Counter]:{AtomicCounterService.MsgSuccessCount}");
//            //                    if (Logger.IsEnabled(LogLevel.Trace)) Logger.LogTrace($"[Accept Hex {session.Client.RemoteEndPoint}]:{package.OriginalData.ToArray().ToHexString()}");
//            //                    SessionManager.TryLink(package.Header.TerminalPhoneNo, session);
//            //                    if (JT808UseType == JT808UseType.Normal)
//            //                    {
//            //                        JT808NormalReplyMessageHandler.Processor(package, session);
//            //                    }
//            //                    else if (JT808UseType == JT808UseType.Queue)
//            //                    {
//            //                        MsgProducer.ProduceAsync(package.Header.TerminalPhoneNo, package.OriginalData.ToArray());
//            //                    }
//            //                }
//            //            }
//            //            catch (NotImplementedException ex)
//            //            {
//            //                Logger.LogError(ex.Message);
//            //            }
//            //            catch (JT808Exception ex)
//            //            {
//            //                AtomicCounterService.MsgFailIncrement();
//            //                if (Logger.IsEnabled(LogLevel.Information)) Logger.LogInformation($"[Atomic Fail Counter]:{AtomicCounterService.MsgFailCount}");
//            //                Logger.LogError($"[HeaderDeserialize ErrorCode]:{ ex.ErrorCode},[ReaderBuffer]:{contentSpan.ToArray().ToHexString()}");
//            //            }
//            //            totalConsumed += (seqReader.Consumed - totalConsumed);
//            //            if (seqReader.End) break;
//            //            seqReader.Advance(1);
//            //            mark = 0;
//            //        }
//            //        mark++;
//            //    }
//            //    else
//            //    {
//            //        seqReader.Advance(1);
//            //    }
//            //}
//            //if (seqReader.Length == totalConsumed)
//            //{
//            //    examined = consumed = buffer.End;
//            //}
//            //else
//            //{
//            //    consumed = buffer.GetPosition(totalConsumed);
//            //}
//        }

//    }
//}
