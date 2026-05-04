using DataAccess.Interfaces;
using TESTAvaloniaApplication.BusinessLayer.Services;
using TESTAvaloniaApplication.BusinessLayer.Models;

namespace TestProject1;


//Hvad bliver der bliver testet her:
//Her testes PressureLogic2 klassen med leaky bucket algoritmen
//og state maskinen. Som skal sikre at følgende scenarier virker:

//1. Alarm udløses — hårdt, vedvarende tryk på ét punkt fylder spanden op (som også testes i testsimulator
//2. Ingen alarm — svag støj under NOISE_FLOOR (10%) fylder ikke spanden
//3. Recovery — når trykket fjernes, falder spanden og alarmen stopper
//4. Alarm på hel kolonne — fire samtidige trykkende punkter fylder alle spande op, uden at påvirke nabopunkterne
//
//Hvorfor bruger vi 0.1 ticksecond
// PressureLogic2 bruger tid til at styre hvor meget der hældes i spanden.
// I det rigtige program beregnes tiden automatisk — men i en test kører koden
// så hurtigt at tiden er næsten 0, og spandene aldrig fyldes.
// Vi sender derfor 0.1 ind manuelt, som svarer til 100ms — præcis som timeren gør.

//Den falske sensor
//Begge sensorer implementerer ISensorReader — det samme interface som den
//rigtige hardware bruger. PressureLogic2 kan ikke se forskel på dem.

//fixed sensor: Returnerer altid den samme matrix.
//Bruges i støjtesten hvor sensoren aldrig må ændre sig, for at se om spandene bliver fyldt op ved støj.

//her bliver fixedsensor skabt
class FixedSensor : ISensorReader
{
    private readonly int[,] _matrix;
    public FixedSensor(int[,] matrix) => _matrix = matrix;
    public int[,] ReadMatrix() => (int[,])_matrix.Clone();
}

// MUTABLE SENSOR: Returnerer den matrix der ligger i Matrix-variablen.
// Vi kan skifte Matrix undervejs i testen — fx fra baseline til højtryk.
// Bruges i alle tests undtagen støjtesten.
class MutableSensor : ISensorReader
{
    public int[,] Matrix { get; set; }
    public MutableSensor(int[,] matrix) => Matrix = matrix;
    public int[,] ReadMatrix() => (int[,])Matrix.Clone();
}


public class Tests
{
    // =========================================================================
    // HJÆLPEMETODER til at bygge testmatrixer
    // =========================================================================

    // Tom måtte — alle 16 punkter har værdien 1000 (ingen belastning)
    private static int[,] Baseline()
    {
        var m = new int[4, 4];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                m[r, c] = 1000;
        return m;
    }

    // Måtte med tryk på ét punkt (0,0) — resten er baseline
    // Procentvis forskel: ((5000 - 1000) / 1000) * 100 = 400%
    private static int[,] HojtTryk()
    {
        var m = Baseline();
        m[0, 0] = 200;
        return m;
    }

    // Måtte med tryk langs en hel kolonne — alle fire rækker i kolonnen
    // Procentvis forskel for hvert punkt: ((5000 - 1000) / 1000) * 100 = 400%
    private static int[,] KolonneTryk(int c)
    {
        var m = Baseline();
        for (int r = 0; r < 4; r++)
            m[r, c] =  200;
        return m;
    }

    // Hjælpemetode der kører systemet igennem Init og Kalibrering med tom måtte,
    // og returnerer sensoren så vi kan skifte til højtryk bagefter.
    // Alle fire tests starter på samme måde — denne metode undgår gentagelse.
    private static (PressureLogic2 logic, MutableSensor sensor) KlargorSystem()
    {
        var sensor = new MutableSensor(Baseline());
        var logic = new PressureLogic2(sensor);

        logic.RunStateMachineTick(0.1); // Init       → systemet gør sig klar
        logic.RunStateMachineTick(0.1); // Kalibrering → reference sættes til 1000

        return (logic, sensor);
    }


    // =========================================================================
    // SCENARIE 1: Alarm udløses ved hårdt, vedvarende tryk på ét punkt
    //
    // Forløb:
    //   Trin 1 — Kalibrering med tom måtte (reference = 1000)
    //   Trin 2 — Punkt (0,0) presses til 5000 → ratio = 400%
    //   Trin 3 — Spanden stiger med (400 - 20) * 0.1 = 38 per tick
    //   Trin 4 — Efter ~8 ticks er spanden fyldt (≥ 300) → Alarm
    // =========================================================================
    [Test]
    public void Alarm_Udloeses_Ved_Vedvarende_Hoejt_Tryk()
    {
        var (logic, sensor) = KlargorSystem();

        // Person sætter sig tungt på punkt (0,0)
        sensor.Matrix = HojtTryk();
        for (int i = 0; i < 55; i++)
            logic.RunStateMachineTick(0.1);

        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Alarm));
    }


    // =========================================================================
    // SCENARIE 2: Ingen alarm ved støj under NOISE_FLOOR
    //
    // Forløb:
    //   Sensor returnerer altid 1000 — samme som reference
    //   Ratio = ((1000 - 1000) / 1000) * 100 = 0%
    //   0% er under NOISE_FLOOR (10%) → intet hældes i spanden → ingen alarm
    // =========================================================================
    [Test]
    public void Ingen_Alarm_Ved_Stoej_Under_Noise_Floor()
    {
        // FixedSensor bruges her fordi sensoren aldrig må skifte værdi
        var sensor = new FixedSensor(Baseline());
        var logic = new PressureLogic2(sensor);

        // 50 ticks — langt mere end nødvendigt for at bekræfte ingen alarm kommer
        for (int i = 0; i < 50; i++)
            logic.RunStateMachineTick(0.1);

        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Monitorering));
    }


    // =========================================================================
    // SCENARIE 3: Systemet vender tilbage til Monitorering når tryk fjernes
    //
    // Forløb:
    //   Trin 1 — Kør til alarm (samme som scenarie 1)
    //   Trin 2 — Person rejser sig → sensor skifter til baseline
    //   Trin 3 — Spanden falder fra 300 til 298 på næste tick (under grænsen)
    //          → ingen aktiv alarm → state skifter tilbage til Monitorering
    // =========================================================================
    [Test]
    public void System_Vender_Tilbage_Til_Monitorering_Naar_Tryk_Fjernes()
    {
        var (logic, sensor) = KlargorSystem();

        // Kør til alarm
        sensor.Matrix = HojtTryk();
        for (int i = 0; i < 55; i++)
            logic.RunStateMachineTick(0.1);

        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Alarm),
            "Alarm skulle have været udløst inden recovery-test");

        // Person rejser sig — ét tick er nok til at spanden falder under grænsen
        sensor.Matrix = Baseline();
        logic.RunStateMachineTick(0.1);

        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Monitorering));
    }


    // =========================================================================
    // SCENARIE 4: Alarm udløses ved tryk langs en hel kolonne
    //
    // Forløb:
    //   Trin 1 — Kalibrering med tom måtte (reference = 1000)
    //   Trin 2 — Alle fire punkter i kolonne 0 presses til 5000
    //   Trin 3 — Alle fire spande fyldes op simultant → Alarm
    //
    // Vi tjekker tre ting:
    //   1. State er Alarm
    //   2. Alle fire spande i kolonne 0 er fyldt op til 300
    //   3. Kolonne 1 er upåvirket — algoritmen "smitter" ikke til nabopunkter
    // =========================================================================
    [Test]
    public void Alarm_Udloeses_Ved_Tryk_Langs_Hel_Kolonne()
    {
        var (logic, sensor) = KlargorSystem();

        // Hele kolonne 0 presses
        sensor.Matrix = KolonneTryk(c: 0);
        for (int i = 0; i < 55; i++)
            logic.RunStateMachineTick(0.1);

        var spande = logic.GetBuckets();

        // 1. Systemet skal være i Alarm
        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Alarm));

        // 2. Alle fire punkter i kolonne 0 skal være fyldt op til 300
        Assert.That(spande[0, 0], Is.EqualTo(300.0), "Række 0, kolonne 0 burde være fyldt");
        Assert.That(spande[1, 0], Is.EqualTo(300.0), "Række 1, kolonne 0 burde være fyldt");
        Assert.That(spande[2, 0], Is.EqualTo(300.0), "Række 2, kolonne 0 burde være fyldt");
        Assert.That(spande[3, 0], Is.EqualTo(300.0), "Række 3, kolonne 0 burde være fyldt");

        // 3. Kolonne 1 må ikke være påvirket
        Assert.That(spande[0, 1], Is.EqualTo(0.0), "Kolonne 1 burde ikke være påvirket");
    }
}
