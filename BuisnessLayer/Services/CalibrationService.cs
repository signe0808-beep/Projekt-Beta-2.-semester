using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Services
{
    public class CalibrationService
        //denne klasse skal kun gemme referenceMatrix og udregne start modstanden ud
    {
        private double[,] _referenceMatrix = new double[4, 4];

        public void SetBaseline(int[,] currentMatrix)
        {
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    //sikrer at vi aldrig gemmer 0 (for man må ikke dividere med 0 senere) det sidste del af koden er en nem if/else
                    _referenceMatrix[r, c] = currentMatrix[r, c] == 0 ? 1.0 : currentMatrix[r, c];
                }
            }
            
        }
        public double[,] GetBaseline()
        {
            return _referenceMatrix;
        }

    }
}
