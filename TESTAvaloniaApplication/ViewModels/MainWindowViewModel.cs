using BusinessLayer.Services;
using TESTAvaloniaApplication.DataAccess.Simulators;
using TESTAvaloniaApplication.BusinessLayer.Models;
using Avalonia.Threading;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TESTAvaloniaApplication.DataAccess.Drivers;

namespace TESTAvaloniaApplication.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private PressureMonitor _minMotor;

        [ObservableProperty]
        private string _statusText = "Status: Normal";

        public MainWindowViewModel()
        {
            var minFalskeSensor = new HardwareMatrixReader(); //her skiftes til hardwarematrixReader
            _minMotor = new PressureMonitor(minFalskeSensor);

            // Opdaterer StatusText hvert 100ms baseret på systemets tilstand
            DispatcherTimer.Run(() =>
            {
                StatusText = _minMotor.CurrentState == SystemStateEnum.Alarm
                    ? "Status: ALARM!"
                    : "Status: Normal";
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
