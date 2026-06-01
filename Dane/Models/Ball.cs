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

        public double X
        {
            get { lock (_lock) return _x; }
            set { lock (_lock) _x = value; }
        }

        public double Y
        {
            get { lock (_lock) return _y; }
            set { lock (_lock) _y = value; }
        }

        public double Radius { get; }
        public double Mass { get; }

        public double VelocityX
        {
            get { lock (_lock) return _velocityX; }
            set { lock (_lock) _velocityX = value; }
        }

        public double VelocityY
        {
            get { lock (_lock) return _velocityY; }
            set { lock (_lock) _velocityY = value; }
        }

        public bool IsMoving { get; private set; }

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

        public void StartMoving(Table table, double intervalMs)
        {
            StopMoving();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;
            IsMoving = true;

            _movementTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    double deltaTime = intervalMs / 1000.0;

                    double newX = X + VelocityX * deltaTime;
                    double newY = Y + VelocityY * deltaTime;

                    // Odbicie od ścian
                    if (newX < 0)
                    {
                        VelocityX = Math.Abs(VelocityX);
                        newX = 0;
                    }
                    else if (newX + Radius > table.Width)
                    {
                        VelocityX = -Math.Abs(VelocityX);
                        newX = table.Width - Radius;
                    }

                    if (newY < 0)
                    {
                        VelocityY = Math.Abs(VelocityY);
                        newY = 0;
                    }
                    else if (newY + Radius > table.Height)
                    {
                        VelocityY = -Math.Abs(VelocityY);
                        newY = table.Height - Radius;
                    }

                    X = newX;
                    Y = newY;

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