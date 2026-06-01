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

        {
            Radius = radius;
        }

        {
        }
    }
}