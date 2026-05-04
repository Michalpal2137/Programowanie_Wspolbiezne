using System;
using System.Collections.Generic;

namespace Logika
{
    public interface IBallService
    {
        event Action<IEnumerable<(double X, double Y, double Radius)>>? BallsUpdated;
        void CreateBalls(int count);
        void ClearBalls();
        void StartSimulation(double intervalMs);
        void StopSimulation();
        (double Width, double Height) GetTableDimensions();
    }
}