using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dane;

namespace Logika
{
    public class BallService : IBallService
    {
        private readonly IBallRepository _repository;
        private readonly Random _random;
        private readonly object _collisionLock = new object();
        private Timer? _collisionTimer;
        private double _minVelocity = 30;
        private double _maxVelocity = 100;
        private double _intervalMs = 16;
        private DateTime _lastFrameTime;

        
        private IDiagnosticWriter? _diagnosticWriter;
        private bool _diagnosticsEnabled;
        private long _frameNumber;
        private long _deadlinesMissed;
        private double _totalDeltaTime;
        private double _targetFrameTimeMs = 16.0;
        private double _deadlineThresholdMs = 20.0;

        public event Action<IEnumerable<(double X, double Y, double Radius)>>? BallsUpdated;

        public bool IsDiagnosticsEnabled => _diagnosticsEnabled;

        public BallService(IBallRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _random = new Random();
        }

        public void EnableDiagnostics()
        {
            if (!_diagnosticsEnabled)
            {
                string logPath = $"diagnostics_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                _diagnosticWriter = new DiagnosticWriter(logPath);
                _diagnosticsEnabled = true;
                _frameNumber = 0;
                _deadlinesMissed = 0;
                _totalDeltaTime = 0;
            }
        }

        public void DisableDiagnostics()
        {
            if (_diagnosticsEnabled && _diagnosticWriter is IDisposable disposable)
            {
                disposable.Dispose();
                _diagnosticWriter = null;
                _diagnosticsEnabled = false;
            }
        }

        public DiagnosticSummary? GetDiagnosticSummary()
        {
            if (!_diagnosticsEnabled || _diagnosticWriter == null)
                return null;

            return new DiagnosticSummary
            {
                TotalFrames = _frameNumber,
                DeadlinesMissed = _deadlinesMissed,
                AverageDeltaTime = _frameNumber > 0 ? _totalDeltaTime / _frameNumber : 0,
                CurrentQueueSize = _diagnosticWriter.QueueSize
            };
        }

        public void CreateBalls(int count)
        {
            _repository.Clear();
            var table = _repository.GetTable();
            double radius = 15;

            for (int i = 0; i < count; i++)
            {
                double x = radius + _random.NextDouble() * (table.Width - 2 * radius);
                double y = radius + _random.NextDouble() * (table.Height - 2 * radius);

                double velocityX = (_random.NextDouble() * 2 - 1) *
                                   (_minVelocity + _random.NextDouble() * (_maxVelocity - _minVelocity));
                double velocityY = (_random.NextDouble() * 2 - 1) *
                                   (_minVelocity + _random.NextDouble() * (_maxVelocity - _minVelocity));

                double mass = radius * radius;

                var ball = new Ball(x, y, radius, mass, velocityX, velocityY);
                ball.PositionChanged += OnBallPositionChanged;
                _repository.AddBall(ball);
            }
        }

        public void ClearBalls()
        {
            var balls = _repository.GetAllBalls();
            foreach (var ball in balls)
            {
                ball.PositionChanged -= OnBallPositionChanged;
            }
            _repository.Clear();
        }

        public void StartSimulation(double intervalMs)
        {
            StopSimulation();

            _intervalMs = intervalMs;
            _targetFrameTimeMs = intervalMs;
            _deadlineThresholdMs = intervalMs * 1.25; 
            _lastFrameTime = DateTime.Now;

            _repository.StartAllBalls(intervalMs);

            
            _collisionTimer = new Timer(
                callback: OnTimerTick,
                state: null,
                dueTime: 0,                    
                period: (int)intervalMs        
            );
        }

        private void OnTimerTick(object? state)
        {
            var frameStart = DateTime.Now;
            var deltaTime = (frameStart - _lastFrameTime).TotalMilliseconds;
            _lastFrameTime = frameStart;

            
            CheckCollisions();

            var frameEnd = DateTime.Now;
            var elapsed = (frameEnd - frameStart).TotalMilliseconds;

            
            bool deadlineMet = elapsed <= _deadlineThresholdMs;
            if (!deadlineMet)
            {
                Interlocked.Increment(ref _deadlinesMissed);
            }

            Interlocked.Increment(ref _frameNumber);
            _totalDeltaTime += elapsed;

            
            if (_diagnosticsEnabled && _diagnosticWriter != null)
            {
                var balls = _repository.GetAllBalls().ToList();
                var diagnosticData = new DiagnosticData
                {
                    Timestamp = DateTime.Now,
                    BallCount = balls.Count,
                    FrameNumber = _frameNumber,
                    DeltaTime = elapsed,
                    DeadlineMet = deadlineMet,
                    Balls = balls.Select(b => new BallSnapshot
                    {
                        X = b.X,
                        Y = b.Y,
                        VelocityX = b.VelocityX,
                        VelocityY = b.VelocityY,
                        Radius = b.Radius,
                        Mass = b.Mass
                    }).ToList()
                };

                
                _ = _diagnosticWriter.WriteAsync(diagnosticData);
            }
        }

        public void StopSimulation()
        {
            _repository.StopAllBalls();

            if (_collisionTimer != null)
            {
                _collisionTimer.Change(Timeout.Infinite, Timeout.Infinite);
                _collisionTimer.Dispose();
                _collisionTimer = null;
            }
        }

        private void CheckCollisions()
        {
            lock (_collisionLock)
            {
                var balls = _repository.GetAllBalls().ToList();

                for (int i = 0; i < balls.Count; i++)
                {
                    for (int j = i + 1; j < balls.Count; j++)
                    {
                        HandleCollision(balls[i], balls[j]);
                    }
                }
            }
        }

        private void HandleCollision(Ball ball1, Ball ball2)
        {
            double dx = ball2.X - ball1.X;
            double dy = ball2.Y - ball1.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            double minDistance = ball1.Radius + ball2.Radius;

            if (distance < minDistance && distance > 0.0001)
            {
                double nx = dx / distance;
                double ny = dy / distance;

                double dvx = ball1.VelocityX - ball2.VelocityX;
                double dvy = ball1.VelocityY - ball2.VelocityY;
                double vn = dvx * nx + dvy * ny;

                if (vn > 0)
                {
                    double m1 = ball1.Mass;
                    double m2 = ball2.Mass;
                    double impulse = 2 * vn / (m1 + m2);

                    
                    ball1.SetVelocity(
                        ball1.VelocityX - impulse * m2 * nx,
                        ball1.VelocityY - impulse * m2 * ny,
                        "BallCollision"
                    );

                    ball2.SetVelocity(
                        ball2.VelocityX + impulse * m1 * nx,
                        ball2.VelocityY + impulse * m1 * ny,
                        "BallCollision"
                    );

                    double overlap = minDistance - distance;
                    double totalMass = m1 + m2;

                    
                    ball1.SetPosition(
                        ball1.X - overlap * (m2 / totalMass) * nx,
                        ball1.Y - overlap * (m2 / totalMass) * ny
                    );

                    ball2.SetPosition(
                        ball2.X + overlap * (m1 / totalMass) * nx,
                        ball2.Y + overlap * (m1 / totalMass) * ny
                    );
                }
            }
        }

        private void OnBallPositionChanged()
        {
            var ballData = new List<(double X, double Y, double Radius)>();
            foreach (var ball in _repository.GetAllBalls())
            {
                ballData.Add((ball.X, ball.Y, ball.Radius));
            }

            BallsUpdated?.Invoke(ballData);
        }

        public (double Width, double Height) GetTableDimensions()
        {
            var table = _repository.GetTable();
            return (table.Width, table.Height);
        }
    }
}