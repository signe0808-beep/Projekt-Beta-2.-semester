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
    /* MEASUREMENT CONTROLLER
    Denne klasse binder hele vores forretningslogik sammen. Den regner ikke selv på tryksår, 
    men fungerer som en State Machine, der styrer hvad der sker hvornår.
    Dens primære opgaver er:
       1. At styre hvor ofte systemet måler via en asynkron Timer  (10 Hz).
       2. At hente data fra hardwaren (SensorReader).
       3. At videresende data til enten kalibrering eller Leaky Bucket-algoritmen 
          afhængig af systemets aktuelle tilstand.
    Ved at samle styringen her, overholder vi Single Responsibility-princippet, 
    så vores matematiske algoritmer kan testes isoleret.
    */
    public class PressureMonitor:IPressureMonitor
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
            // Injicerer hardwaren (gør det muligt at bruge TestSimulator fremfor ægte hardware)
            _sensor = sensor;

            // Opretter vores Business Logic Services 
            _leakyBucketCalculator = new LeakyBucketCalculator();
            _calibrationService= new CalibrationService();

            // Starter vores loop, der kører hvert 100. millisekund (10 Hz)
            _tickTimer = new System.Timers.Timer(100);
            _tickTimer.Elapsed += OnTimerElapsed;
        }

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
            // DELTA TIME:
            // For at sikre at algoritmen kører præcist selvom der er forsinkelser i hardwaren, 
            // beregnes tiden mellem hver måling i sekunder (Δt). Hvis hardwaren oplever lag, 
            // udlignes dette matematisk, så den samlede procentsats altid passer.
            var currentTime = DateTime.Now;
            double deltaTime = (currentTime - _lastTickTime).TotalSeconds;
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
                    _calibrationService.SetBaseline(currentMatrix);
                    CurrentState = SystemStateEnum.Monitorering;
                break;

                case SystemStateEnum.Monitorering:
                case SystemStateEnum.Alarm:

                    //Henter det kalibrerede referenceMatrix
                    double[,] baseline = _calibrationService.GetBaseline();


                    //Bruger Calculator-klassen til at udregne trykket og ser om "spanden flyder over"
                    //Returnerer true, hvis trykket er for højt og "spanden flyder over"
                    bool isAlarm=_leakyBucketCalculator.proccessData(currentMatrix, baseline,deltaTime);

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
