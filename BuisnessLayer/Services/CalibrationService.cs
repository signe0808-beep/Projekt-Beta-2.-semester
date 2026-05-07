using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class CalibrationService
    /* CALIBRATION SERVICE
Denne klasse har ansavret for at finde systemets nulpunkt/Baseline.
Fordi fysiske tryksensoreren lavet med velostat kan ændre grundmodstand
baseret på temperatur, slid eller montering, kan vi ikke antage, 
at en tom måtte altid måler '0'.

Denne klasse gemmer belastningen af måtten i ubelastet tilstand.
Denne matrix bruges senere af vores Leaky Bucket-algoritme til at udregne 
den reelle, procentvise ændring, uanset hvordan hardwaren opfører sig på dagen.
*/
    {

        // En 4x4 matrix, der gemmer startmodstanden for alle 16 sensorpunkter
        private double[,] _referenceMatrix = new double[4, 4];

        // Modtager måling fra hardwaren (når systemet er i Kalibreringstilstand)
        public void SetBaseline(int[,] currentMatrix)
        {
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    //sikrer at vi aldrig gemmer 0 (for man må ikke dividere med 0 senere)
                    _referenceMatrix[r, c] = currentMatrix[r, c] == 0 ? 1.0 : currentMatrix[r, c];
                }
            }
            
        }
        // Laver metode til at få referencematrixen, så 'PressureMonitor' kan hente det
        // og sende det videre til Matematikeren
        public double[,] GetBaseline()
        {
            return _referenceMatrix;
        }

    }
}
