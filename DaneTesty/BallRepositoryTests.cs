using Dane;
using System.Linq;
using Xunit;

namespace DaneTesty
{
    public class BallRepositoryTests
    {
        [Fact]
        public void AddBall_ShouldAddBallToRepository()
        {
            // Arrange
            var repository = new BallRepository(800, 600);
            var ball = new Ball(100, 200, 15, 225, 10, 5);

            // Act
            repository.AddBall(ball);

            // Assert
            Assert.Single(repository.GetAllBalls());
        }

        [Fact]
        public void AddMultipleBalls_ShouldContainAllBalls()
        {
            // Arrange
            var repository = new BallRepository(800, 600);

            // Act
            repository.AddBall(new Ball(100, 200, 15, 225, 10, 5));
            repository.AddBall(new Ball(300, 400, 20, 400, 5, 10));
            repository.AddBall(new Ball(500, 100, 10, 100, -5, 15));

            // Assert
            Assert.Equal(3, repository.GetAllBalls().Count());
        }

        [Fact]
        public void Clear_ShouldRemoveAllBalls()
        {
            // Arrange
            var repository = new BallRepository(800, 600);
            repository.AddBall(new Ball(100, 200, 15, 225, 10, 5));
            repository.AddBall(new Ball(300, 400, 20, 400, 5, 10));

            // Act
            repository.Clear();

            // Assert
            Assert.Empty(repository.GetAllBalls());
        }

        [Fact]
        public void UpdateBallPosition_ShouldUpdateCoordinates()
        {
            // Arrange
            var repository = new BallRepository(800, 600);
            var ball = new Ball(100, 200, 15, 225, 10, 5);
            repository.AddBall(ball);

            // Act
            repository.UpdateBallPosition(ball, 300, 400);

            // Assert
            var updatedBall = repository.GetAllBalls().First();
            Assert.Equal(300, updatedBall.X);
            Assert.Equal(400, updatedBall.Y);
        }

        [Fact]
        public void RemoveBall_ShouldRemoveSpecificBall()
        {
            // Arrange
            var repository = new BallRepository(800, 600);
            var ball1 = new Ball(100, 200, 15, 225, 10, 5);
            var ball2 = new Ball(300, 400, 20, 400, 5, 10);
            repository.AddBall(ball1);
            repository.AddBall(ball2);

            // Act
            repository.RemoveBall(ball1);

            // Assert
            Assert.Single(repository.GetAllBalls());
            Assert.Equal(300, repository.GetAllBalls().First().X);
        }

        [Fact]
        public void GetTable_ShouldReturnCorrectDimensions()
        {
            // Arrange
            var repository = new BallRepository(800, 600);

            // Act
            var table = repository.GetTable();

            // Assert
            Assert.Equal(800, table.Width);
            Assert.Equal(600, table.Height);
        }
    }
}