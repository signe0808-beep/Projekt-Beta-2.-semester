using System;
using System.Timers;
using System.Diagnostics;
using TESTAvaloniaApplication.BusinessLayer.Models;

namespace TESTAvaloniaApplication.BusinessLayer.Services
{
    internal class PressureLogic2
    {
        //Systemets tilstand
        public SystemStateEnum CurrentState { get; private set; } = SystemStateEnum.Initialisering;

        //Timer og Delta Time
        private Timer _tickTimer;
        private DateTime _lastTickTime;

        // variable opgraderet til 4x4 bruger "leaky-bucket metoden"
        private double[,] _buckets = new double[4, 4]; // 16 "spande", der holder på "skaden"
        private const double DECAY_CONSTANT = 5.0;     // Hvor meget der siver ud af spanden pr. sek
        private const double ALARM_THRESHOLD = 100.0;  // Grænsen for alarm (Makkerens timeThreshold)
        private const int NOISE_FLOOR = 15;            // Makkerens pressureThreshold

        // (Eventuelt et objekt til at snakke med hardwaren)
        // private ISensorReader _sensor; 

        public PressureLogic2()
        {
            // Starter vores loop, der kører f.eks. hvert 100 millisekund
            _tickTimer = new Timer(100);
            _tickTimer.Elapsed += RunStateMachineTick;
        }

        public void StartSystem()
        {
            _lastTickTime = DateTime.Now;
            _tickTimer.Start();
        }

        // SELVE STATE MACHINEN OG LOGIKKEN (skal stå her splittet op i stedet for at være samlet)
        private void RunStateMachineTick(object sender, ElapsedEventArgs e)
        {
            // Beregn Delta Time (Hvor lang tid er der gået siden sidste måling)
            var currentTime = DateTime.Now;
            double deltaTime = (currentTime - _lastTickTime).TotalSeconds;
            _lastTickTime = currentTime;

            // Hent data (Fakes her indtil hardware er sat til)
            int[,] currentMatrix = new int[4, 4]; // Her kalder vi normalt _sensor.ReadMatrix();

            bool anyBucketCritical = false;

            switch (CurrentState)
            {
                case SystemStateEnum.Initialisering:
                    // Gør systemet klar
                    CurrentState = SystemStateEnum.Kalibrering;
                    break;

                case SystemStateEnum.Kalibrering:
                    // Gem referenceværdier 
                    CurrentState = SystemStateEnum.Monitorering;
                    break;

                case SystemStateEnum.Monitorering:
                case SystemStateEnum.AlarmAktiv:

                    // Løb gennem alle 16 punkter på måtten
                    for (int r = 0; r < 4; r++)
                    {
                        for (int c = 0; c < 4; c++)
                        {
                            double pressure = currentMatrix[r, c];

                            // Fjern støj
                            if (pressure < NOISE_FLOOR) pressure = 0;

                            // Hæld i spanden
                            _buckets[r, c] += (pressure * deltaTime);

                            // Siv ud af spanden
                            _buckets[r, c] -= (DECAY_CONSTANT * deltaTime);

                            // Sørg for at spanden ikke går under 0
                            if (_buckets[r, c] < 0) _buckets[r, c] = 0;

                            // Tjek om denne specifikke spand flyder over!
                            if (_buckets[r, c] >= ALARM_THRESHOLD)
                            {
                                _buckets[r, c] = ALARM_THRESHOLD;
                                anyBucketCritical = true;
                            }
                        }
                    }

                    // SKIFT TILSTAND BASERET PÅ SPANDENE!
                    if (anyBucketCritical)
                    {
                        CurrentState = SystemStateEnum.AlarmAktiv;
                    }
                    else
                    {
                        // Hvis ingen spande er fulde mere, går vi automatisk ud af alarm
                        CurrentState = SystemStateEnum.Monitorering;
                    }
                    break;
            }
        }
    }
}