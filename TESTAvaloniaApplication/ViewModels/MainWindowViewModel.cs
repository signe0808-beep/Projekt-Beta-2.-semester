using BusinessLayer.Services;
using TESTAvaloniaApplication.DataAccess.Simulators;
using TESTAvaloniaApplication.BusinessLayer.Models;
using Avalonia.Threading;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Presentation.ViewModels;

namespace TESTAvaloniaApplication.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private PressureMonitor _minMotor;

        [ObservableProperty]
        private string _statusText = "Status: Normal";

        [ObservableProperty]
        private string _statusColor = "#4CAF50";

        public HeatmapViewModel HeatmapData { get; }

        public MainWindowViewModel()
        {
            var minFalskeSensor = new TestSimulator();
            _minMotor = new PressureMonitor(minFalskeSensor);
            HeatmapData = new HeatmapViewModel(_minMotor);

            // Opdaterer StatusText og farve hvert 100ms baseret på systemets tilstand
            DispatcherTimer.Run(() =>
            {
                bool visAlarm = _minMotor.CurrentState == SystemStateEnum.Alarm;

                StatusText = visAlarm
                    ? "Alarm aktiveret\rændrer position"
                    : "Status: Normal";

                StatusColor = visAlarm ? "#F44336" : "#4CAF50";

                return true;
            }, TimeSpan.FromMilliseconds(100));
        }

        [RelayCommand]
        private void StartCalibration()
        {
            _minMotor.StartSystem();
        }
    }
}
