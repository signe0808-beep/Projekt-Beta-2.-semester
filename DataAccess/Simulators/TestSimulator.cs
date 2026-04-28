using DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TESTAvaloniaApplication.DataAccess.Simulators
{
    // Vi skriver under på kontrakten med ": ISensorReader"
    public class TestSimulator : ISensorReader
    {
        private int _counter = 0;
        private Random _rand = new Random();

        public int[,] ReadMatrix()
        {
            int[,] matrix = new int[4, 4];
            _counter++;

            // 1. Fyld stolen med "tom" støj (ca. 1000 ohm) overalt
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    matrix[r, c] = _rand.Next(950, 1050);
                }
            }

            // 2. Skab en cyklus der går fra 0 til 99, og så starter forfra
            int cyklus = _counter % 100;

            if (cyklus < 50)
            {
                // De første 50 omgange: Vægten stiger og stiger på punkt (1,1)
                matrix[1, 1] = 1000 + (cyklus * 80);
            }
            else
            {
                // De næste 50 omgange: Personen har rejst sig! (Kun støj tilbage)
                matrix[1, 1] = 1000;
            }

            return matrix;
        }
    }
}
