using BusinessLayer.Services;
using TESTAvaloniaApplication.DataAccess.Simulators;
using TESTAvaloniaApplication.BusinessLayer.Models;
using TESTAvaloniaApplication.BusinessLayer.Interfaces;
using Avalonia.Threading;
using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Presentation.ViewModels;

namespace TESTAvaloniaApplication.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private IPressureMonitor _minMotor;

        private bool _systemStartet = false;
        private bool _isKalibreret = false;
        private bool _erPauset = false;
        private DateTime _kalibreretTidspunkt = DateTime.MinValue;

        [ObservableProperty]
        private string _statusText = "Tryk start for at begynde";

        [ObservableProperty]
        private string _statusColor = "#2196F3";

        [ObservableProperty]
        private string _knapTekst = "Start";

        public HeatmapViewModel HeatmapData { get; }

        public MainWindowViewModel(IPressureMonitor motor)
        {
            _minMotor = motor;

            HeatmapData = new HeatmapViewModel(_minMotor);

            DispatcherTimer.Run(() =>
            {
                if (!_systemStartet)
                {
                    StatusText = "Tryk start for at begynde";
                    StatusColor = "#2196F3";
                    KnapTekst = "Start";
                }
                else if (_erPauset)
                {
                    StatusText = "Måling sat på pause";
                    StatusColor = "#FFC107";
                    KnapTekst = "Start igen";
                }
                else if (!_isKalibreret)
                {
                    if (_minMotor.CurrentState == SystemStateEnum.Monitorering)
                    {
                        _isKalibreret = true;
                        _kalibreretTidspunkt = DateTime.Now;
                    }
                    StatusText = "Kalibrerer system - vent med at sætte dig";
                    StatusColor = "#FFC107";
                    KnapTekst = "Stop";
                }
                else if (_minMotor.CurrentState == SystemStateEnum.Alarm)
                {
                    StatusText = "Alarm aktiveret - rejs dig fra siddemåtten";
                    StatusColor = "#F44336";
                    KnapTekst = "Stop";
                }
                else if ((DateTime.Now - _kalibreretTidspunkt).TotalSeconds < 5)
                {
                    StatusText = "System kalibreret og klar til brug";
                    StatusColor = "#4CAF5" +
                    "0";
                    KnapTekst = "Stop";
                }
                else
                {
                    StatusText = "Ingen alarm";
                    StatusColor = "#4CAF50";
                    KnapTekst = "Stop";
                }

                return true;
            }, TimeSpan.FromMilliseconds(100));
        }

        [RelayCommand]
        private void ToggleSystem()
        {
            if (!_systemStartet)
            {
                // Første gang — kalibrér og start
                _systemStartet = true;
                _isKalibreret = false;
                _erPauset = false;
                _minMotor.StartSystem();
            }
            else if (!_erPauset)
            {
                // Stop målingerne midlertidigt
                _erPauset = true;
                _minMotor.StopSystem();
            }
            else
            {
                // Genoptag uden kalibrering
                _erPauset = false;
                _minMotor.ResumeSystem();
            }
        }
    }
}
