using Dane;
using Logika;
using System.Linq;
using Xunit;

namespace LogikaTesty
{
    public class BallServiceTests
    {
        private IBallRepository CreateRepository()
        {
            return new BallRepository(800, 600);
        }

        [Fact]
        public void CreateBalls_ShouldCreateCorrectNumberOfBalls()
        {
            // Arrange
            var repository = CreateRepository();
            var service = new BallService(repository);

            // Act
            service.CreateBalls(5);

            // Assert
            Assert.Equal(5, repository.GetAllBalls().Count());
        }

        [Fact]
        public void CreateBalls_ShouldPositionBallsWithinTableBounds()
        {
            // Arrange
            var repository = CreateRepository();
            var service = new BallService(repository);

            // Act
            service.CreateBalls(10);

            // Assert
            foreach (var ball in repository.GetAllBalls())
            {
                Assert.True(ball.X >= 0);
                Assert.True(ball.X + ball.Radius <= 800);
                Assert.True(ball.Y >= 0);
                Assert.True(ball.Y + ball.Radius <= 600);
            }
        }

        [Fact]
        public void ClearBalls_ShouldRemoveAllBalls()
        {
            // Arrange
            var repository = CreateRepository();
            var service = new BallService(repository);
            service.CreateBalls(5);

            // Act
            service.ClearBalls();

            // Assert
            Assert.Empty(repository.GetAllBalls());
        }

        [Fact]
        public void CreateBalls_WithZeroCount_ShouldCreateNoBalls()
        {
            // Arrange
            var repository = CreateRepository();
            var service = new BallService(repository);

            // Act
            service.CreateBalls(0);

            // Assert
            Assert.Empty(repository.GetAllBalls());
        }

        [Fact]
        public void GetTableDimensions_ShouldReturnCorrectValues()
        {
            // Arrange
            var repository = CreateRepository();
            var service = new BallService(repository);

            // Act
            var dimensions = service.GetTableDimensions();

            // Assert
            Assert.Equal(800, dimensions.Width);
            Assert.Equal(600, dimensions.Height);
        }

        [Fact]
        public void StartSimulation_ShouldTriggerBallsUpdatedEvent()
        {
            // Arrange
            var repository = CreateRepository();
            var service = new BallService(repository);
            service.CreateBalls(3);

            bool eventTriggered = false;
            service.BallsUpdated += (balls) => eventTriggered = true;

            // Act
            service.StartSimulation(100);
            System.Threading.Thread.Sleep(200);

            // Assert
            Assert.True(eventTriggered);

            // Cleanup
            service.StopSimulation();
        }

        [Fact]
        public void StopSimulation_ShouldStopUpdates()
        {
            // Arrange
            var repository = CreateRepository();
            var service = new BallService(repository);
            service.CreateBalls(3);

            int eventCount = 0;
            service.BallsUpdated += (balls) => eventCount++;

            // Act
            service.StartSimulation(50);
            System.Threading.Thread.Sleep(100);
            service.StopSimulation();
            int countAfterStop = eventCount;
            System.Threading.Thread.Sleep(200);

            // Assert
            Assert.True(countAfterStop > 0);
            Assert.Equal(countAfterStop, eventCount);
        }

        [Fact]
        public void CreateBalls_ShouldAssignVelocity()
        {
            // Arrange
            var repository = CreateRepository();
            var service = new BallService(repository);

            // Act
            service.CreateBalls(5);

            // Assert
            foreach (var ball in repository.GetAllBalls())
            {
                Assert.NotEqual(0, ball.VelocityX);
                Assert.NotEqual(0, ball.VelocityY);
            }
        }
    }
}