using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dane
{
    public class Ball
    {
        private readonly object _lock = new object();
        private double _x;
        private double _y;
        private double _velocityX;
        private double _velocityY;
        private Task? _movementTask;
        private CancellationTokenSource? _cancellationTokenSource;
        private DateTime _lastFrameTime;
        private long _deadlinesMissed;
        private long _totalFrames;
        private double _totalExecutionTime;

        public double X
        {
            get { lock (_lock) return _x; }
            private set { lock (_lock) _x = value; }
        }

        public double Y
        {
            get { lock (_lock) return _y; }
            private set { lock (_lock) _y = value; }
        }

        public double Radius { get; }
        public double Mass { get; }

        public double VelocityX
        {
            get { lock (_lock) return _velocityX; }
            private set { lock (_lock) _velocityX = value; }
        }

        public double VelocityY
        {
            get { lock (_lock) return _velocityY; }
            private set { lock (_lock) _velocityY = value; }
        }

        public bool IsMoving { get; private set; }

        public long DeadlinesMissed => Interlocked.Read(ref _deadlinesMissed);
        public long TotalFrames => Interlocked.Read(ref _totalFrames);
        public double AverageExecutionTime => _totalFrames > 0 ? _totalExecutionTime / _totalFrames : 0;

        public event Action? PositionChanged;

        public Ball(double x, double y, double radius, double mass, double velocityX, double velocityY)
        {
            _x = x;
            _y = y;
            Radius = radius;
            Mass = mass;
            _velocityX = velocityX;
            _velocityY = velocityY;
        }

      
        public void SetVelocity(double vx, double vy, string source)
        {
            lock (_lock)
            {
                
                if (double.IsNaN(vx) || double.IsInfinity(vx) ||
                    double.IsNaN(vy) || double.IsInfinity(vy))
                    return;

                
                double maxVelocity = 500;
                _velocityX = Math.Clamp(vx, -maxVelocity, maxVelocity);
                _velocityY = Math.Clamp(vy, -maxVelocity, maxVelocity);
            }
        }

        public void SetPosition(double x, double y)
        {
            lock (_lock)
            {
                if (double.IsNaN(x) || double.IsInfinity(x) ||
                    double.IsNaN(y) || double.IsInfinity(y))
                    return;

                _x = x;
                _y = y;
            }
        }

        public void StartMoving(Table table, double intervalMs, double deadlineThresholdMs = 20.0)
        {
            StopMoving();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            IsMoving = true;
            _lastFrameTime = DateTime.Now;

            _movementTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var frameStart = DateTime.Now;

                    double deltaTime = intervalMs / 1000.0;

                    double newX, newY;

                    lock (_lock)
                    {
                        newX = _x + _velocityX * deltaTime;
                        newY = _y + _velocityY * deltaTime;
                    }

                    
                    if (newX < 0)
                    {
                        SetVelocity(Math.Abs(_velocityX), _velocityY, "WallCollision");
                        newX = 0;
                    }
                    else if (newX + Radius > table.Width)
                    {
                        SetVelocity(-Math.Abs(_velocityX), _velocityY, "WallCollision");
                        newX = table.Width - Radius;
                    }

                    if (newY < 0)
                    {
                        SetVelocity(_velocityX, Math.Abs(_velocityY), "WallCollision");
                        newY = 0;
                    }
                    else if (newY + Radius > table.Height)
                    {
                        SetVelocity(_velocityX, -Math.Abs(_velocityY), "WallCollision");
                        newY = table.Height - Radius;
                    }

                    SetPosition(newX, newY);

                    
                    var frameEnd = DateTime.Now;
                    var elapsed = (frameEnd - frameStart).TotalMilliseconds;

                    Interlocked.Increment(ref _totalFrames);
                    _totalExecutionTime += elapsed;

                    if (elapsed > deadlineThresholdMs)
                    {
                        Interlocked.Increment(ref _deadlinesMissed);
                    }

                    PositionChanged?.Invoke();

                    try
                    {
                        await Task.Delay((int)intervalMs, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
                IsMoving = false;
            }, token);
        }

        public void StopMoving()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                _movementTask = null;
                IsMoving = false;
            }
        }
    }
}