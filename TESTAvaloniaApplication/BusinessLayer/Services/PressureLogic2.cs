using System;
using System.Diagnostics;
using System.Timers;
using TESTAvaloniaApplication.BusinessLayer.Models;
using static TESTAvaloniaApplication.BusinessLayer.Interfaces.IPressureLogic2; //gør det muligt at implementerer Interfaces

namespace TESTAvaloniaApplication.BusinessLayer.Services
{
    /* 
 SYSTEMARKITEKTUR OG BUSINESSLOGIC
  
 Denne klasse er logikken bag tryksystemet. Den skal aflæse rådata fra matrixen (4*4) 
 filtrere støj, udregne risikoen tryksår og opdatere brugergrænsefladen.
 Logikken er bygget op omkring tre trin:
    1.STATE MACHINE
 Systemet kører på en asynkron Timer, der afvikler et "tick" med en fast frekvens 
 (fx 10 Hz, dette skal bare rettes til). Maskinen skifter 
 mellem fire tilstande: Initialisering, Kalibrering (Tare), Monitoring og AlarmAktiv.
    2. "LEAKY BUCKET" ALGORITMEN (SPANDE-METODEN)
 For at håndtere alle 16 sensorpunkter på samme tid, uden at skulle styre 16 
 stopure, bruger vi "Leaky Bucket" metodenm. Hvert punkt på måtten fungerer som 
 en vandspand med et hul i bunden:
 - Påfyldning: Når et tryk registreres (over et defineret støj-niveau), 
 lægges trykket ned i spanden for det specifikke punkt.
 - Aflastning: Samtidig trækkes der konstant en lille aflastningsværdi ud af alle spande. 
 - Resultat: Hvis et punkt belastes konstant, fyldes spanden op. Rammer den 100 %, 
 udløses alarmen. Flyttes trykket, falder værdien langsomt mod 0 igen (derfor aflastningen). Dette filtrerer 
 naturligt små ryk og stillingsskift fra uden at nulstille den samlede belastning.
    3. DELTA TIME
 For at sikre at algoritmen kan køre trods forsinkelser i hardwaren, benyttes Delta Time (Δt).
 Tiden mellem hver måling beregnes præcist i sekunder. Alt påfyldning og aflastning i spandene ganges med 
 denne Delta Time. Hvis hardwaren oplever lag, udlignes dette matematisk, så den samlede 
 procentsats altid passer med det faktiske antal sekunder, borgeren har siddet på måtten. 
 */
    internal class PressureLogic2 //: IPressureLogic2 //- mangler referance??
    {
        //Systemets tilstand
        public SystemStateEnum CurrentState { get; private set; } = SystemStateEnum.Initialisering;

        //Timer og Delta Time
        private Timer _tickTimer;
        private DateTime _lastTickTime; //DataTime er indbygget, og tager det præcise øjeblik

        // variable opgraderet til 4x4 bruger "leaky-bucket metoden"
        private double[,] _buckets = new double[4, 4]; // 16 "spande", der holder på "skaden"
        private const double DECAY_CONSTANT = 5.0;     // Hvor meget der siver ud af spanden pr. sek SKAL EVT RETTES, DETTE ER TILFÆLDIGT TAL
        private const double ALARM_THRESHOLD = 100.0;  // Grænsen for alarm (Makkerens timeThreshold) SKAL HELT SIKKERT OGSÅ RETTES
        private const int NOISE_FLOOR = 15;            // Makkerens pressureThreshold SKAL MÅLES OG RETTES EFTER

        // (Eventuelt et objekt til at snakke med hardwaren)
        // private ISensorReader _sensor; 

        public PressureLogic2()
        {
            // Starter vores loop, der kører fx hvert 100 millisekund
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

                            // Tjek om denne specifikke spand flyder over
                            if (_buckets[r, c] >= ALARM_THRESHOLD)
                            {
                                _buckets[r, c] = ALARM_THRESHOLD;
                                anyBucketCritical = true;
                            }
                        }
                    }

                    // Skift tilstand baseret på spandene
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