using System;
using System.Collections.Generic;

namespace Dane
{
    [Serializable]
    public class DiagnosticData
    {
        public DateTime Timestamp { get; set; }
        public int BallCount { get; set; }
        public List<BallSnapshot> Balls { get; set; } = new List<BallSnapshot>();
        public long FrameNumber { get; set; }
        public double DeltaTime { get; set; }
        public bool DeadlineMet { get; set; }
    }

    [Serializable]
    public class BallSnapshot
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double VelocityX { get; set; }
        public double VelocityY { get; set; }
        public double Radius { get; set; }
        public double Mass { get; set; }
    }
}