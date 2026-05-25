using DataAccess.Interfaces;

namespace TESTAvaloniaApplication.DataAccess.Simulators
{
    // Simulerer en person der sætter sig og rejser sig i en 100-tick cyklus.
    // Bruges til at teste systemet visuelt på en computer uden RPi.
    public class TestSimulator : ISensorReader
    {
        // Tæller ticks for at holde styr på hvilken fase i cyklussen vi er i
        private int _counter = 0;

        // Bruges til at generere tilfældig baggrundsstøj på alle 16 punkter
        private Random _rand = new Random();

        public int[,] ReadMatrix()
        {
            int[,] matrix = new int[4, 4];
            _counter++;

            // Alle punkter får tilfældig støj (0-2) som simulerer en rigtig sensors naturlige variation
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    matrix[r, c] = _rand.Next(0, 3);

            // Cyklussen gentages hver 100 ticks — de første 50 sidder personen, de næste 50 er rejst
            int cyklus = _counter % 100;

            if (cyklus < 50)
            {
                // Personen sætter sig — tre punkter stiger gradvist fra 1 op mod deres maksimum
                matrix[1, 1] = Math.Min(150, 1 + (cyklus * 3));
                matrix[2, 2] = Math.Min(130, 1 + (cyklus * 3));
                matrix[1, 3] = Math.Min(120, 1 + (cyklus * 2));
            }
            else
            {
                // Personen rejser sig — punkterne falder tilbage til baseline
                matrix[1, 1] = 1;
                matrix[2, 2] = 1;
                matrix[1, 3] = 1;
            }

            return matrix;
        }
    }
}
