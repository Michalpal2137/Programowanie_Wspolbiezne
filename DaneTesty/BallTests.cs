using Dane;
using Xunit;

namespace DaneTesty
{
    public class BallTests
    {
        [Fact]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            // Act 
            var ball = new Ball(100, 200, 15, 225, 10, 5);

            // Assert
            Assert.Equal(100, ball.X);
            Assert.Equal(200, ball.Y);
            Assert.Equal(15, ball.Radius);
            Assert.Equal(225, ball.Mass);
            Assert.Equal(10, ball.VelocityX);
            Assert.Equal(5, ball.VelocityY);
        }

        [Fact]
        public void SetVelocity_ShouldUpdateVelocity()
        {
            // Arrange
            var ball = new Ball(100, 200, 15, 225, 10, 5);

            // Act
            ball.SetVelocity(50, -30, "Test");

            // Assert
            Assert.Equal(50, ball.VelocityX);
            Assert.Equal(-30, ball.VelocityY);
        }

        [Fact]
        public void SetVelocity_ShouldRejectNaN()
        {
            // Arrange
            var ball = new Ball(100, 200, 15, 225, 10, 5);

            // Act
            ball.SetVelocity(double.NaN, double.NaN, "Test");

            // Assert
            Assert.Equal(10, ball.VelocityX);
            Assert.Equal(5, ball.VelocityY);
        }

        [Fact]
        public void SetPosition_ShouldUpdatePosition()
        {
            // Arrange
            var ball = new Ball(100, 200, 15, 225, 10, 5);

            // Act
            ball.SetPosition(300, 400);

            // Assert
            Assert.Equal(300, ball.X);
            Assert.Equal(400, ball.Y);
        }

        [Fact]
        public void SetPosition_ShouldRejectInvalidValues()
        {
            // Arrange
            var ball = new Ball(100, 200, 15, 225, 10, 5);

            // Act
            ball.SetPosition(double.NaN, double.PositiveInfinity);

            // Assert 
            Assert.Equal(100, ball.X);
            Assert.Equal(200, ball.Y);
        }
    }
}