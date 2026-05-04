using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dane
{
    public class BallRepository : IBallRepository
    {
        private readonly ObservableCollection<Ball> _balls;
        private readonly Table _table;

        public BallRepository(double tableWidth, double tableHeight)
        {
            _table = new Table(tableWidth, tableHeight);
            _balls = new ObservableCollection<Ball>();
        }

        public void AddBall(Ball ball)
        {
            _balls.Add(ball);
        }

        public void RemoveBall(Ball ball)
        {
            _balls.Remove(ball);
        }

        public IEnumerable<Ball> GetAllBalls()
        {
            return _balls.ToList();
        }

        public void Clear()
        {
            _balls.Clear();
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
    }
}