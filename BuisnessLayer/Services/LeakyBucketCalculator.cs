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
        private double[,] _buckets = new double[4, 4]; // Laver et 4x4 array, som skaber de 16 spande
    
        //vi modtager den færdig kalibrerede matrix
        public bool proccessData(double[,] calibratedMatrix, double deltaTime)
        {
            bool anyBucketCritical = false;
            // Løb gennem alle 16 punkter på måtten
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    //Vi tager værdien direkte fra den kalibrerede matrix   
                    double pressure = calibratedMatrix[r, c];

                    // Fjern støj
                    if (pressure < SystemConstants.NOISE_FLOOR) pressure = 0;

                    // Hælder i spanden, hvor deltatime måler tiden mellem to målinger på 100ms,
                    // men tager højde for hvis der sker en forsinkelse
                    _buckets[r, c] += (pressure * deltaTime);

                    //Eksponentiel tømning af spanden
                    _buckets[r, c] -= (_buckets[r,c] * SystemConstants.DECAY_FACTOR * deltaTime);

                    // Sørg for at spanden ikke går under 0
                    if (_buckets[r, c] < 0) _buckets[r, c] = 0;

                    // Tjek om denne specifikke spand flyder over/ går over alarmgrænsen
                    if (_buckets[r, c] >= SystemConstants.ALARM_THRESHOLD)
                    {
                        _buckets[r, c] = SystemConstants.ALARM_THRESHOLD;
                        anyBucketCritical = true;
                    }
                }
            }
            return anyBucketCritical; //returnere sand hvis der er alarm og falsk hvis den skal blive i monitorering - bruges i pressuremonitor

        }
        //Returnerer de private spande så skærmen kan læse dem
        public double[,] GetBuckets()
        {
            return _buckets;
        }
    }

}
