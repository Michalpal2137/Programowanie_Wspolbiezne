using System;
using System.Windows;
using Dane;
using Logika;
using View;
using ViewModel;

namespace Programowanie_Wspolbiezne
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var repository = new BallRepository(800, 600);
            var ballService = new BallService(repository);
            var viewModel = new MainViewModel(ballService);
            
            var app = new Application();
            var mainWindow = new MainWindow();
            mainWindow.DataContext = viewModel;
            
            app.Run(mainWindow);
        }
    }
}