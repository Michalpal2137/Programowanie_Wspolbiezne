using System;
using System.Collections.Generic;
using Dane;

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

       
        void EnableDiagnostics();
        void DisableDiagnostics();
        bool IsDiagnosticsEnabled { get; }
        DiagnosticSummary? GetDiagnosticSummary();
    }

    public class DiagnosticSummary
    {
        public long TotalFrames { get; set; }
        public long DeadlinesMissed { get; set; }
        public double AverageDeltaTime { get; set; }
        public int CurrentQueueSize { get; set; }
    }
}