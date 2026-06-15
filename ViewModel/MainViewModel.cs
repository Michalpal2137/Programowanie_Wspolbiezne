using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Linq;
using System.Windows.Threading;
using Logika;
using Model;

namespace ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IBallService _ballService;
        private readonly Dispatcher _dispatcher;
        private int _ballCount = 10;
        private bool _isSimulationRunning;
        private bool _isDiagnosticsEnabled;
        private double _tableWidth = 800;
        private double _tableHeight = 600;
        

        public ObservableCollection<BallModel> Balls { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public int BallCount
        {
            get => _ballCount;
            set
            {
                if (_ballCount != value && value > 0)
                {
                    _ballCount = value;
                    OnPropertyChanged(nameof(BallCount));
                }
            }
        }

        public bool IsSimulationRunning
        {
            get => _isSimulationRunning;
            set
            {
                if (_isSimulationRunning != value)
                {
                    _isSimulationRunning = value;
                    OnPropertyChanged(nameof(IsSimulationRunning));
                    ((RelayCommand)StartCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)StopCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)CreateBallsCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsDiagnosticsEnabled
        {
            get => _isDiagnosticsEnabled;
            set
            {
                if (_isDiagnosticsEnabled != value)
                {
                    _isDiagnosticsEnabled = value;
                    OnPropertyChanged(nameof(IsDiagnosticsEnabled));
                    OnPropertyChanged(nameof(DiagnosticStatus));
                }
            }
        }

        public string DiagnosticStatus
        {
            get
            {
                if (IsDiagnosticsEnabled)
                {
                    var summary = _ballService.GetDiagnosticSummary();
                    if (summary != null)
                    {
                        return $"Kl: {summary.TotalFrames} | Śr: {summary.AverageDeltaTime:F1}ms | " +
                               $"Pominięte: {summary.DeadlinesMissed} | Kol: {summary.CurrentQueueSize}";
                    }
                }
                return "Diagnostyka wyłączona";
            }
        }

        public double TableWidth
        {
            get => _tableWidth;
            set
            {
                _tableWidth = value;
                OnPropertyChanged(nameof(TableWidth));
            }
        }

        public double TableHeight
        {
            get => _tableHeight;
            set
            {
                _tableHeight = value;
                OnPropertyChanged(nameof(TableHeight));
            }
        }

        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand CreateBallsCommand { get; private set; }
        public ICommand ToggleDiagnosticsCommand { get; private set; }
        public ICommand RefreshDiagnosticsCommand { get; private set; }

        public MainViewModel(IBallService ballService)
        {
            _ballService = ballService ?? throw new ArgumentNullException(nameof(ballService));
            _dispatcher = Dispatcher.CurrentDispatcher;
            Balls = new ObservableCollection<BallModel>();

            StartCommand = new RelayCommand(StartSimulation, () => !IsSimulationRunning);
            StopCommand = new RelayCommand(StopSimulation, () => IsSimulationRunning);
            CreateBallsCommand = new RelayCommand(CreateBalls, () => !IsSimulationRunning);
            ToggleDiagnosticsCommand = new RelayCommand(ToggleDiagnostics);
            RefreshDiagnosticsCommand = new RelayCommand(RefreshDiagnostics);

            var dimensions = _ballService.GetTableDimensions();
            TableWidth = dimensions.Width;
            TableHeight = dimensions.Height;

            _ballService.BallsUpdated += OnBallsUpdated;
        }

        private async void StartSimulation()
        {
            IsSimulationRunning = true;
            await System.Threading.Tasks.Task.Run(() => _ballService.StartSimulation(16));
        }

        private async void StopSimulation()
        {
            await System.Threading.Tasks.Task.Run(() => _ballService.StopSimulation());
            IsSimulationRunning = false;
        }

        private void CreateBalls()
        {
            _ballService.CreateBalls(_ballCount);
            _dispatcher.Invoke(() => Balls.Clear());
        }

        private void ToggleDiagnostics()
        {
            if (IsDiagnosticsEnabled)
            {
                _ballService.DisableDiagnostics();
                IsDiagnosticsEnabled = false;
            }
            else
            {
                _ballService.EnableDiagnostics();
                IsDiagnosticsEnabled = true;
            }
        }

        private void RefreshDiagnostics()
        {
            OnPropertyChanged(nameof(DiagnosticStatus));
        }

        private void OnBallsUpdated(IEnumerable<(double X, double Y, double Radius)> ballsData)
        {
            var ballsList = ballsData.ToList();

            _dispatcher.Invoke(() =>
            {
                while (Balls.Count < ballsList.Count)
                {
                    Balls.Add(new BallModel());
                }

                while (Balls.Count > ballsList.Count)
                {
                    Balls.RemoveAt(Balls.Count - 1);
                }

                for (int i = 0; i < ballsList.Count; i++)
                {
                    Balls[i].X = ballsList[i].X;
                    Balls[i].Y = ballsList[i].Y;
                    Balls[i].Radius = ballsList[i].Radius;
                }

                
                if (IsDiagnosticsEnabled)
                {
                    OnPropertyChanged(nameof(DiagnosticStatus));
                }
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}