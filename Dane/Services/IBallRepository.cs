using System.Collections.Generic;

namespace Dane
{
    public interface IBallRepository
    {
        void AddBall(Ball ball);
        void RemoveBall(Ball ball);
        IEnumerable<Ball> GetAllBalls();
        void Clear();
        void UpdateBallPosition(Ball ball, double x, double y);
        Table GetTable();
        void StartAllBalls(double intervalMs);
        void StopAllBalls();
    }
}