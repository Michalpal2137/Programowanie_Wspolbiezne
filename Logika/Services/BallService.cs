using Dane;
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

                double velocityX = (_random.NextDouble() * 2 - 1) *
                                   (_minVelocity + _random.NextDouble() * (_maxVelocity - _minVelocity));
                double velocityY = (_random.NextDouble() * 2 - 1) *
                                   (_minVelocity + _random.NextDouble() * (_maxVelocity - _minVelocity));

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

        {
            {
            }
        {
        }
                }
            }, token);
        }

        {

            {
                }
                else if (newX + ball.Radius > table.Width)
                {
                    ball.VelocityX = -Math.Abs(ball.VelocityX);
                    newX = table.Width - ball.Radius;
                }

                {
                {

                _repository.UpdateBallPosition(ball, newX, newY);
            }
            // Kolizje między kulami
            for (int i = 0; i < balls.Count; i++)
            {
                for (int j = i + 1; j < balls.Count; j++)
                {

        private void HandleCollision(Ball ball1, Ball ball2)
        {
                    double dx = ball2.X - ball1.X;
                    double dy = ball2.Y - ball1.Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double minDistance = ball1.Radius + ball2.Radius;

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

                            // Rozepchnij kule
                            double overlap = minDistance - distance;
                    }
                }
            }

        private void OnBallPositionChanged()
        {
            var ballData = new List<(double X, double Y, double Radius)>();
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