using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dane
{
    public class BallRepository : IBallRepository
    {
        private readonly ObservableCollection<Ball> _balls;
        private readonly Table _table;
        private readonly object _lock = new object();

        public BallRepository(double tableWidth, double tableHeight)
        {
            _table = new Table(tableWidth, tableHeight);
            _balls = new ObservableCollection<Ball>();
        }

        public void AddBall(Ball ball)
        {
            lock (_lock)
            {
            _balls.Add(ball);
        }
        }

        public void RemoveBall(Ball ball)
        {
            lock (_lock)
            {
            _balls.Remove(ball);
        }
        }

        public IEnumerable<Ball> GetAllBalls()
        {
            lock (_lock)
            {
            return _balls.ToList();
        }
        }

        public void Clear()
        {
            StopAllBalls();
            lock (_lock)
            {
            _balls.Clear();
        }
        }

        public void UpdateBallPosition(Ball ball, double x, double y)
        {
            ball.X = x;
            ball.Y = y;
        }

        public Table GetTable()
        {
            return _table;
        }

        public void StartAllBalls(double intervalMs)
        {
            var table = GetTable();
            var balls = GetAllBalls();

            foreach (var ball in balls)
            {
                ball.StartMoving(table, intervalMs);
            }
        }

        public void StopAllBalls()
        {
            var balls = GetAllBalls();

            foreach (var ball in balls)
            {
                ball.StopMoving();
            }
        }
    }
}