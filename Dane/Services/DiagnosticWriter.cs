using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Dane
{
    public class DiagnosticWriter : IDiagnosticWriter, IDisposable
    {
        private readonly BlockingCollection<DiagnosticData> _queue;
        private readonly string _filePath;
        private readonly CancellationTokenSource _cts;
        private readonly Task _writerTask;
        private readonly object _fileLock = new object();
        private int _maxQueueSize = 1000;
        private bool _disposed;

        public bool IsReady => _queue.Count < _maxQueueSize;
        public int QueueSize => _queue.Count;

        public DiagnosticWriter(string filePath)
        {
            _filePath = filePath;
            _queue = new BlockingCollection<DiagnosticData>(_maxQueueSize);
            _cts = new CancellationTokenSource();

            _writerTask = Task.Run(() => ProcessQueueAsync(_cts.Token));
        }

        public Task WriteAsync(DiagnosticData data)
        {
            if (_disposed)
                return Task.CompletedTask;

            
            if (!_queue.TryAdd(data))
            {
                
                data.DeadlineMet = false;
            }

            return Task.CompletedTask;
        }

        private async Task ProcessQueueAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (_queue.TryTake(out var data, 100, token))
                    {
                        await WriteToFileAsync(data);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    
                    await Console.Error.WriteLineAsync($"Diagnostic write error: {ex.Message}");
                }
            }

            
            while (_queue.TryTake(out var remaining))
            {
                try
                {
                    await WriteToFileAsync(remaining);
                }
                catch { }
            }
        }

        private async Task WriteToFileAsync(DiagnosticData data)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                MaxDepth = 10
            };

            string json = JsonSerializer.Serialize(data, options);

            
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(json);
            byte[] asciiBytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, utf8Bytes);
            string asciiString = Encoding.ASCII.GetString(asciiBytes);

            lock (_fileLock)
            {
                File.AppendAllText(_filePath, asciiString + Environment.NewLine);
            }

            await Task.CompletedTask;
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _queue.CompleteAdding();
                _cts.Cancel();

                try
                {
                    _writerTask?.Wait(2000);
                }
                catch (AggregateException) { }

                _cts.Dispose();
                _queue.Dispose();
            }
        }
    }
}