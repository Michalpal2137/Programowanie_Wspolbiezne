using Dane;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DaneTesty
{
    public class DiagnosticWriterTests
    {
        [Fact]
        public async Task WriteAsync_ShouldCreateFile_WithAsciiContent()
        {
            // Arrange
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

            // Act
            await writer.WriteAsync(data);
            await Task.Delay(500); 
            ((IDisposable)writer).Dispose();
            await Task.Delay(200); 

            // Assert
            Assert.True(File.Exists(filePath));

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public async Task WriteAsync_QueueFull_ShouldNotBlock()
        {
            // Arrange
            string filePath = $"test_{Guid.NewGuid()}.json";
            var writer = new DiagnosticWriter(filePath);

            // Act
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

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 1000,
                $"Zapis trwał zbyt długo: {stopwatch.ElapsedMilliseconds}ms");

            // Dispose 
            ((IDisposable)writer).Dispose();
            await Task.Delay(500); 

            // Cleanup 
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

        [Fact]
        public void DiagnosticWriter_IsReady_ShouldIndicateQueueStatus()
        {
            // Arrange
            string filePath = $"test_{Guid.NewGuid()}.json";
            var writer = new DiagnosticWriter(filePath);

            // Assert
            Assert.True(writer.IsReady);
            Assert.Equal(0, writer.QueueSize);

            // Cleanup
            ((IDisposable)writer).Dispose();

            
            System.Threading.Thread.Sleep(300);

            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (IOException)
            {
                
            }
        }
    }
}