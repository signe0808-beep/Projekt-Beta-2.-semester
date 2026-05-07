using DataAccess.Interfaces;

namespace TESTAvaloniaApplication.DataAccess.Simulators
{
    //Her bliver der skabt en fake sensor som simulerer en person som sætter sig ned i et stykke tid og rejser sig igen i en uendelig cyklus
    //Denne her test klasse adskiller sig fra unittesten i testprojekt, da den visuelt tester at programmet kan køres på en computer uden rpi
    //
    // Vi skriver under på kontrakten med ": ISensorReader",
    // så pressurelogic2 kan bruge den præcis som om at det var rigtig hardware.
    public class TestSimulator : ISensorReader
    {
        // Tæller hvor mange gange ReadMatrix er blevet kaldt siden programmet startede.
        // Bruges til at beregne hvilken fase i cyklussen vi er i.
        private int _counter = 0;

        // Bruges til at generere tilfældig støj på alle 16 punkter.
        private Random _rand = new Random();

        // Kaldes af PressureLogic2 ved hvert tick (hvert 100ms).
        // Returnerer en 4x4 matrix der simulerer trykfordelingen på måtten.
        public int[,] ReadMatrix()
        {
            int[,] matrix = new int[4, 4];
            _counter++;

            // TRIN 1: Fyld alle 16 punkter med tilfældig støj (950-1050).
            // Dette simulerer den naturlige variation en rigtig sensor altid vil have,
            // selv når ingen sidder på måtten.
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    matrix[r, c] = _rand.Next(950, 1050);
                }
            }

            // TRIN 2: Beregn hvilken fase af cyklussen vi er i.
            // Cyklussen går fra 0 til 99 og starter forfra — altså 100 ticks per runde.
            // Ved 100ms per tick tager én runde 10 sekunder i det rigtige system.
            int cyklus = _counter % 100;

            if (cyklus < 50)
            {
                // Første halvdel (ticks 0-49): personen sætter sig og trykket stiger.
                // Punkt (1,1) stiger med 80 per tick — fra 1000 op til 4920.
                // Dette simulerer stigende belastning på ét punkt.
                matrix[1, 1] = 1000 + (cyklus * 80);
            }
            else
            {
                // Anden halvdel (ticks 50-99): personen har rejst sig.
                // Punkt (1,1) falder tilbage til baseline (1000) — kun støj tilbage.
                matrix[1, 1] = 1000;
            }

            return matrix;
        }
    }
}
