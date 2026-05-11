using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Models;


namespace BusinessLayer.Services
{
    public class LeakyBucketCalculator
        //denne klasse udregner og holder styr på vores "spande"
    {
        private double[,] _buckets = new double[4, 4]; // 16 "spande", der holder på "skaden"
        //KONSTANTER

    

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
                    if (pressureRatio < 0.0) pressureRatio = 0;

                    // Fjern støj
                    if (pressureRatio < SystemConstants.NOISE_FLOOR) pressureRatio = 0;

                    // Hæld i spanden
                    _buckets[r, c] += (pressureRatio * deltaTime);

                    // Siv ud af spanden
                    _buckets[r, c] -= (SystemConstants.DECAY_CONSTANT * deltaTime);

                    // Sørg for at spanden ikke går under 0
                    if (_buckets[r, c] < 0) _buckets[r, c] = 0;

                    // Tjek om denne specifikke spand flyder over
                    if (_buckets[r, c] >= SystemConstants.ALARM_THRESHOLD)
                    {
                        _buckets[r, c] = SystemConstants.ALARM_THRESHOLD;
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
