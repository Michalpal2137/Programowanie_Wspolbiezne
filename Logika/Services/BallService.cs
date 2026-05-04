using Dane;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Logika
{
    public class BallService : IBallService
    {
        private readonly IBallRepository _repository;
        private readonly Random _random;
        private Timer? _timer;
        private double _minVelocity = 30;
        private double _maxVelocity = 100;

        public event Action<IEnumerable<(double X, double Y, double Radius)>>? BallsUpdated;

        public BallService(IBallRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _random = new Random();
        }

        public void CreateBalls(int count)
        {
            _repository.Clear();
            var table = _repository.GetTable();
            double radius = 15;

            for (int i = 0; i < count; i++)
            {
                // X i Y to lewy górny róg kuli
                double x = _random.NextDouble() * (table.Width - radius);
                double y = _random.NextDouble() * (table.Height - radius);

                double velocityX = (_random.NextDouble() * 2 - 1) *
                                   (_minVelocity + _random.NextDouble() * (_maxVelocity - _minVelocity));
                double velocityY = (_random.NextDouble() * 2 - 1) *
                                   (_minVelocity + _random.NextDouble() * (_maxVelocity - _minVelocity));

                var ball = new Ball(x, y, radius, velocityX, velocityY);
                _repository.AddBall(ball);
            }
        }

        public void ClearBalls()
        {
            _repository.Clear();
        }

        public void StartSimulation(double intervalMs)
        {
            StopSimulation(); // Zatrzymaj istniejący timer jeśli istnieje
            _timer = new Timer(intervalMs);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Start();
        }

        public void StopSimulation()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= OnTimerElapsed;
                _timer.Dispose();
                _timer = null;
            }
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            UpdateBalls();
        }

        private void UpdateBalls()
        {
            var table = _repository.GetTable();
            var balls = _repository.GetAllBalls().ToList();
            double deltaTime = 1.0 / 60;

            foreach (var ball in balls)
            {
                double newX = ball.X + ball.VelocityX * deltaTime;
                double newY = ball.Y + ball.VelocityY * deltaTime;

                // Odbicie od ścian - lewy górny róg
                if (newX < 0)
                {
                    ball.VelocityX = Math.Abs(ball.VelocityX);
                    newX = 0;
                }
                else if (newX + ball.Radius > table.Width)
                {
                    ball.VelocityX = -Math.Abs(ball.VelocityX);
                    newX = table.Width - ball.Radius;
                }

                if (newY < 0)
                {
                    ball.VelocityY = Math.Abs(ball.VelocityY);
                    newY = 0;
                }
                else if (newY + ball.Radius > table.Height)
                {
                    ball.VelocityY = -Math.Abs(ball.VelocityY);
                    newY = table.Height - ball.Radius;
                }

                _repository.UpdateBallPosition(ball, newX, newY);
            }
            // Kolizje między kulami
            for (int i = 0; i < balls.Count; i++)
            {
                for (int j = i + 1; j < balls.Count; j++)
                {
                    var ball1 = balls[i];
                    var ball2 = balls[j];

                    double dx = ball2.X - ball1.X;
                    double dy = ball2.Y - ball1.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double minDistance = ball1.Radius + ball2.Radius;

                    if (distance < minDistance && distance > 0)
                    {
                        // Wektor normalny kolizji
                        double nx = dx / distance;
                        double ny = dy / distance;

                        // Względna prędkość
                        double dvx = ball1.VelocityX - ball2.VelocityX;
                        double dvy = ball1.VelocityY - ball2.VelocityY;

                        // Prędkość względna w kierunku normalnym
                        double vn = dvx * nx + dvy * ny;

                        // Nie rób nic jeśli kule się oddalają
                        if (vn > 0)
                        {
                            // Zakładamy równe masy
                            ball1.VelocityX -= vn * nx;
                            ball1.VelocityY -= vn * ny;
                            ball2.VelocityX += vn * nx;
                            ball2.VelocityY += vn * ny;

                            // Rozepchnij kule
                            double overlap = minDistance - distance;
                            ball1.X -= overlap * nx * 0.5;
                            ball1.Y -= overlap * ny * 0.5;
                            ball2.X += overlap * nx * 0.5;
                            ball2.Y += overlap * ny * 0.5;
                        }
                    }
                }
            }
            var ballData = new List<(double X, double Y, double Radius)>();
            foreach (var ball in balls)
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