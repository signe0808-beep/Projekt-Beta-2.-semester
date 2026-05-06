using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class LeakyBucketCalculator
        //denne klasse udregner og holder styr på vores "spande"
    {
        private double[,] _buckets = new double[4, 4]; // 16 "spande", der holder på "skaden"
        //KONSTANTER

        //Hvor hurtigt spand tømmes, her med 100% i sekundet
        private const double DECAY_CONSTANT = 20.0;     // Hvor meget der siver ud af spanden pr. sek SKAL EVT RETTES, DETTE ER TILFÆLDIGT TAL
                                                        //dette betyder at punktet skal ændres med 5000% for alarm
        private const double ALARM_THRESHOLD = 300.0;  // Grænsen for alarm (timeThreshold) SKAL HELT SIKKERT OGSÅ RETTES
                                                       //dette tal betyder nu at punktet skal ændre modstand med mindst 15%
        private const int NOISE_FLOOR = 10;            // pressureThreshold SKAL MÅLES OG RETTES EFTER

        public bool proccessData(int[,] currentMatrix, double[,] _referenceMatrix, double deltaTime)
        {
            bool anyBucketCritical = false;
            // Løb gennem alle 16 punkter på måtten
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    double rawPressure = currentMatrix[r, c];
                    double reference = _referenceMatrix[r, c];
                    
                    //Vi måler den procentvise forskel. Lille adc-tal = højt tryk:
                    //((GammeltTal-Nyttal)/GammeltTakl)*100
                    double pressureRatio = ((reference - rawPressure) / reference) * 100.0;

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
            return anyBucketCritical;

        }
        //Returnerer de private spande så skærmen kan læse dem
        public double[,] GetBuckets()
        {
            return _buckets;
        }
    }

}
