using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TESTAvaloniaApplication.BusinessLayer.Interfaces;
using TESTAvaloniaApplication.BusinessLayer.Models;
using DataAccess.Interfaces;
using System.Timers;
using System.Security.Cryptography.X509Certificates;

namespace BusinessLayer.Services
{
    public class PressureMonitor : IPressureMonitor
    {

        // Systemets aktuelle tilstand (Initialisering, Kalibrering, Monitorering, Alarm). 
        // Den er public 'get', så brugergrænsefladen kan læse med og opdatere skærmen.
        public SystemStateEnum CurrentState { get; private set; } = SystemStateEnum.Initialisering;

        // Timer og tidsstyring
        private System.Timers.Timer _tickTimer;

        // Bruges til at udregne den præcise tidsforskel mellem målinger
        private DateTime _lastTickTime;

        // Referencer til vores Interfaces og services
        private ISensorReader _sensor;
        private CalibrationService _calibrationService;
        private LeakyBucketCalculator _leakyBucketCalculator;

        public PressureMonitor(ISensorReader sensor)
        {
            //Dependency injektion (Gør at programmet er ligeglad med om det data er fra hardware eller testsimulator)
            _sensor = sensor;

            // Opretter vores Business Logic Services 
            _leakyBucketCalculator = new LeakyBucketCalculator();
            _calibrationService= new CalibrationService();

            // Starter vores loop, der kører hvert 100. millisekund (10 Hz)
            _tickTimer = new System.Timers.Timer(100);
            _tickTimer.Elapsed += OnTimerElapsed; //Sørger for at OnTimerElapsed lyttes efter og kaldes hver gang timeren udløses
        }

        //følgende 3 bruges i mainWindowViewModel.cs
        //Kaldes fra UI, når målingen skal begynde
        public void StartSystem()
        {
            _lastTickTime = DateTime.Now;
            _tickTimer.Start();
        }

        //Stopper målingerne midlertidigt
        public void StopSystem()
        {
            _tickTimer.Stop();
        }

        //Genoptager målingerne uden at kalibrere igen
        public void ResumeSystem()
        {
            _lastTickTime = DateTime.Now; // nulstil så deltaTime ikke giver et stort hop
            _tickTimer.Start();
        }

        public void RunStateMachineTick(double deltaTime)
        {
            ExecuteTick(deltaTime); // testen kalder ExecuteTick via denne
        }

        //udløses automatisk hver gang timeren "ticks"
        private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var currentTime = DateTime.Now;
            double deltaTime = (currentTime - _lastTickTime).TotalSeconds; //sørger for at håndtere evt. forsinkelse
            _lastTickTime = currentTime;

            //Kører selve logikken med den udregnede tid
            ExecuteTick(deltaTime);
        }
      
        private void ExecuteTick(double deltaTime)
        {
            //Henter data fra matrixen via vores hardware
            int[,] currentMatrix = _sensor.ReadMatrix();

            //Tjekker hvilken tilstand systemet er i, og udfører handling derefter:
            switch (CurrentState)
            {
                case SystemStateEnum.Initialisering:
                    //gør systemet klar og går videre til kalibrering når der trykkes på knappen
                    CurrentState = SystemStateEnum.Kalibrering;
                break;

                case SystemStateEnum.Kalibrering:
                    //Her skal måtten være tom. Vi gemmer referenceværdier, 
                    //for at kompensere for de naturlige varitioner i sensoren.
                    _calibrationService.SetDailyBaseline(currentMatrix);
                    CurrentState = SystemStateEnum.Monitorering;
                break;

                case SystemStateEnum.Monitorering:
                case SystemStateEnum.Alarm:

                    //Ny matrix til de kalibrederede værdier
                    double[,] calibratedMatrix = new double[4, 4];
                    // Vi kører alle 16 punkter igennem kalibreringen først

                    for (int r = 0; r < 4; r++)
                    {
                        for (int c = 0; c < 4; c++)
                        {
                            calibratedMatrix[r, c] = _calibrationService.GetCalibratedPressure(r, c, currentMatrix[r, c]);
                        }
                    }

                    bool isAlarm = _leakyBucketCalculator.proccessData(calibratedMatrix, deltaTime);

                    // Skift tilstand baseret på spandenes niveau
                    if (isAlarm)
                    {
                        CurrentState = SystemStateEnum.Alarm;
                    }
                    else
                    {
                        // Hvis ingen spande er i kritisk niveau længere, forlades alarmtilstanden
                        CurrentState = SystemStateEnum.Monitorering;
                    }

                break;
            }
        }
        public double[,] GetBuckets()
        {
            // Gør det muligt for skærmens ViewModel at hente de færdigregnede procenter til heatmappet

            return _leakyBucketCalculator.GetBuckets();
        }
    }
}
