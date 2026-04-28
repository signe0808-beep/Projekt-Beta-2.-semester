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

            // 1. Fyld stolen med "tom" støj (ca. 1000 ohm)
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    matrix[r, c] = _rand.Next(950, 1050);
                }
            }

            // 2. Simuler at trykket stiger langsomt på ét punkt
            matrix[1, 1] = 1000 + (_counter * 50);

            return matrix;
        }
    }
}
