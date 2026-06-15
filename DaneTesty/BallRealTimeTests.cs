using Dane;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DaneTesty
{
    public class BallRealTimeTests
    {
        [Fact]
        public void SetVelocity_ShouldClampMaxVelocity()
        {
            // Arrange
            var ball = new Ball(100, 200, 15, 225, 50, 30);

            // Act
            ball.SetVelocity(1000, -1000, "Test");

            // Assert
            Assert.True(ball.VelocityX <= 500);
            Assert.True(ball.VelocityY >= -500);
        }

        [Fact]
        public async Task StartMoving_ShouldTrackDeadlines()
        {
            // Arrange
            var ball = new Ball(100, 200, 15, 225, 50, 30);
            var table = new Table(800, 600);

            // Act
            ball.StartMoving(table, 16, 10);
            await Task.Delay(200);
            ball.StopMoving();

            // Assert
            Assert.True(ball.TotalFrames > 0);
        }
    }
}