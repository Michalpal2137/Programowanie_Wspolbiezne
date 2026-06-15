using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Dane;

namespace LogikaTesty
{
    public class DiagnosticWriterTests
    {
        [Fact]
        public async Task WriteAsync_ShouldCreateFile_WithAsciiContent()
        {
            string filePath = $"test_{Guid.NewGuid()}.json";
            var writer = new DiagnosticWriter(filePath);

            var data = new DiagnosticData
            {
                Timestamp = DateTime.Now,
                BallCount = 5,
                FrameNumber = 1,
                DeltaTime = 16.0,
                DeadlineMet = true
            };

            await writer.WriteAsync(data);
            await Task.Delay(500);
            ((IDisposable)writer).Dispose();
            await Task.Delay(200);

            Assert.True(File.Exists(filePath));
            File.Delete(filePath);
        }

        [Fact]
        public async Task WriteAsync_QueueFull_ShouldNotBlock()
        {
            string filePath = $"test_{Guid.NewGuid()}.json";
            var writer = new DiagnosticWriter(filePath);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < 2000; i++)
            {
                var data = new DiagnosticData
                {
                    Timestamp = DateTime.Now,
                    BallCount = 10,
                    FrameNumber = i,
                    DeltaTime = 16.0,
                    DeadlineMet = true
                };
                await writer.WriteAsync(data);
            }

            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 1000);

            ((IDisposable)writer).Dispose();
            await Task.Delay(500);

            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        break;
                    }
                }
                catch (IOException)
                {
                    await Task.Delay(200);
                }
            }
        }
    }
}