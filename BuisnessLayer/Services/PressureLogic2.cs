using System;
using System.Diagnostics;
using System.Timers;
using TESTAvaloniaApplication.BusinessLayer.Interfaces;
using TESTAvaloniaApplication.BusinessLayer.Models;
 //gør det muligt at implementerer Interfaces


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
    public class PressureLogic2:IPressureLogic2 //- mangler referance??
    {
        //Systemets tilstand
        public SystemStateEnum CurrentState { get; private set; } = SystemStateEnum.Initialisering;

        //Timer og Delta Time
        private System.Timers.Timer _tickTimer;
        private DateTime _lastTickTime; //DataTime er indbygget, og tager det præcise øjeblik

        // variable opgraderet til 4x4 bruger "leaky-bucket metoden"
        private double[,] _buckets = new double[4, 4]; // 16 "spande", der holder på "skaden"
        private double[,] _referenceMatrix = new double[4, 4]; //her gemmer vi startmodstanden ved kalibrering

        //KONSTANTER

        //Hvor hurtigt spand tømmes, her med 100% i sekundet
        private const double DECAY_CONSTANT = 100.0;     // Hvor meget der siver ud af spanden pr. sek SKAL EVT RETTES, DETTE ER TILFÆLDIGT TAL
       //dette betyder at punktet skal ændres med 5000% for alarm
        private const double ALARM_THRESHOLD = 5000.0;  // Grænsen for alarm (timeThreshold) SKAL HELT SIKKERT OGSÅ RETTES
       //dette tal betyder nu at punktet skal ændre modstand med mindst 15%
        private const int NOISE_FLOOR = 15;            // Makkerens pressureThreshold SKAL MÅLES OG RETTES EFTER

        // (Eventuelt et objekt til at snakke med hardwaren)
        private ISensorReader _sensor; 

        public PressureLogic2(ISensorReader sensor)
        {
            // Starter vores loop, der kører fx hvert 100 millisekund
            _tickTimer = new System.Timers.Timer(100);
            _tickTimer.Elapsed += OnTimerElapsed; //seperat metode til timer
            _sensor = sensor;
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            RunStateMachineTick(); //kalder den public metode
        }

        public void StartSystem()
        {
            _lastTickTime = DateTime.Now;
            _tickTimer.Start();
        }

        // SELVE STATE MACHINEN OG LOGIKKEN (skal stå her splittet op i stedet for at være samlet)
        public void RunStateMachineTick()
        {
            // Beregn Delta Time (Hvor lang tid er der gået siden sidste måling)
            var currentTime = DateTime.Now;
            double deltaTime = (currentTime - _lastTickTime).TotalSeconds;
            _lastTickTime = currentTime;

            // Hent data (Fakes her indtil hardware er sat til)
            int[,] currentMatrix = new int[4, 4]; // Her kalder vi normalt _sensor.ReadMatrix(); Altså int[,] currentMatrix = _sensor.ReadMatrix();

            bool anyBucketCritical = false;

            switch (CurrentState)
            {
                case SystemStateEnum.Initialisering:
                    // Gør systemet klar
                    CurrentState = SystemStateEnum.Kalibrering;
                    break;

                case SystemStateEnum.Kalibrering:
                    // Her er der ingen vægt på velostat. Gem referenceværdier for hvert punkt
                    for (int r = 0; r < 4; r++)
                    {
                        for (int c = 0; c < 4; c++)
                        {
                            //sikrer at vi aldrig gemmer 0 (for man må ikke dividere med 0 senere) det sidste del af koden er en nem if/else
                            _referenceMatrix[r,c] = currentMatrix[r,c] == 0 ? 1.0 : currentMatrix[r,c];
                        }
                    }
                    CurrentState = SystemStateEnum.Monitorering;
                    break;

                case SystemStateEnum.Monitorering:
                case SystemStateEnum.Alarm:

                    // Løb gennem alle 16 punkter på måtten
                    for (int r = 0; r < 4; r++)
                    {
                        for (int c = 0; c < 4; c++)
                        {
                            double rawPressure = currentMatrix[r, c];
                            double reference = _referenceMatrix[r, c];
                            //Nyt fra HW, er at vi skal måle procentvis forskel, derfor:
                            //Formel ((NytTal - GammeltTal) / GammeltTal) * 100
                            double pressureRatio = ((rawPressure - reference) / reference) * 100.0;
                            // Hvis trykket er faldet under kalibreringen (pga. hardware støj eller andet), sætter vi det til 0
                            if (pressureRatio < 0.0) rawPressure = 0;

                            // Fjern støj
                            if (pressureRatio < NOISE_FLOOR) pressureRatio = 0;

                            // Hæld i spanden
                            _buckets[r, c] += (pressureRatio * deltaTime);

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
                        CurrentState = SystemStateEnum.Alarm;
                    }
                    else
                    {
                        // Hvis ingen spande er fulde mere, går vi automatisk ud af alarm
                        CurrentState = SystemStateEnum.Monitorering;
                    }
                    break;
            }
        }
        //Returnerer de private spande så skærmen kan læse dem
        public double[,] GetBuckets()
        {
            return _buckets;
        }

    }
}