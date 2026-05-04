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
            var ball = new Ball(100, 200, 15, 10, 5);

            // Assert
            Assert.Equal(100, ball.X);
            Assert.Equal(200, ball.Y);
            Assert.Equal(15, ball.Radius);
            Assert.Equal(10, ball.VelocityX);
            Assert.Equal(5, ball.VelocityY);
        }

        [Fact]
        public void Move_ShouldUpdatePosition()
        {
            // Arrange
            var ball = new Ball(100, 200, 15, 10, 5);

            // Act
            ball.Move(1.0);

            // Assert
            Assert.Equal(110, ball.X);
            Assert.Equal(205, ball.Y);
        }

        [Fact]
        public void Move_WithFractionalTime_ShouldUpdatePosition()
        {
            // Arrange
            var ball = new Ball(100, 200, 15, 10, 5);

            // Act
            ball.Move(0.5);

            // Assert
            Assert.Equal(105, ball.X);
            Assert.Equal(202.5, ball.Y);
        }
    }
}