using System.Runtime.Intrinsics.X86;
using BusinessLayer.Services;
using DataAccess.Interfaces;
using TESTAvaloniaApplication.BusinessLayer.Models;

namespace TestProject1;

// FakeSensor implementerer ISensorReader så PressureMonitor tror det er den rigtige hardware
class FakeSensor : ISensorReader
{
    // Matrixen der skiftes undervejs i testen for at simulere forskellige situationer
    public int[,] Matrix { get; set; }

    // Tager en startmatrix som input
    public FakeSensor(int[,] matrix)
    {
        Matrix = matrix;
    }

    // Kaldes af PressureMonitor ved hvert tick for at hente sensordata
    public int[,] ReadMatrix()
    {
        return Matrix;
    }
}

public class Tests
{
    // Test 1: Alarm udløses ved vedvarende højt tryk på ét punkt
    [Test]
    public void Alarm_Udloeses_Ved_Vedvarende_Hojt_Tryk()
    {
        var sensor = new FakeSensor(TomMåtte());
        var logic = new PressureMonitor(sensor); // sensor leverer tal, logic beregner -> alarm eller ej

        logic.RunStateMachineTick(0.1); // Init til Kalibrering
        logic.RunStateMachineTick(0.1); // Kalibrering til Monitorering

        sensor.Matrix = HojtTryk(); // simulerer højt tryk på punkt (0,0)

        // 103 ticks gør at spand ca. er 4989, lige under grænsen derfor ingen alarm
        for (int i = 0; i < 103; i++)
            logic.RunStateMachineTick(0.1);
        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Monitorering));

        // 1 tick mere (104 i alt) gør at spand overstiger 5000 derfor alarm
        logic.RunStateMachineTick(0.1);
        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Alarm));
    }

    // Test 2: Ingen alarm når trykket er under noise floor
    [Test]
    public void Ingen_Alarm_Ved_Stoej_Under_Noise_Floor()
    {
        var sensor = new FakeSensor(TomMåtte());
        var logic = new PressureMonitor(sensor);

        // 50 ticks med tom måtte
        for (int i = 0; i < 50; i++)
            logic.RunStateMachineTick(0.1);
        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Monitorering));

        // Boundary test 1: adc=2 på ca. 4g < NOISE_FLOOR (15g) - filtreres, spand forbliver 0
        var m = new int[4, 4];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                m[r, c] = 2;
        sensor.Matrix = m;
        for (int i = 0; i < 100; i++)
            logic.RunStateMachineTick(0.1);

        var spande = logic.GetBuckets();
        Assert.That(spande[0, 0], Is.EqualTo(0.0));
        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Monitorering));

        // Boundary test 2: adc=5 -> 18g > NOISE_FLOOR (15g) -> spanden fyldes op, men ingen alarm
        m[0, 0] = 5;
        sensor.Matrix = m;
        for (int i = 0; i < 100; i++)
            logic.RunStateMachineTick(0.1);

        var spande2 = logic.GetBuckets();
        Assert.That(spande2[0, 0], Is.GreaterThan(0.0)); // spanden fylder op
        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Monitorering)); // men ingen alarm
    }

    // Test 3: Systemet vender tilbage til Monitorering når trykket fjernes
    [Test]
    public void System_Vender_Tilbage_Til_Monitorering_Naar_Tryk_Fjernes()
    {
        var sensor = new FakeSensor(TomMåtte());
        var logic = new PressureMonitor(sensor);

        logic.RunStateMachineTick(0.1); 
        logic.RunStateMachineTick(0.1); 

        sensor.Matrix = HojtTryk(); // simulerer at en person sætter sig
        for (int i = 0; i < 104; i++) //alarm udløses ved tick 104
            logic.RunStateMachineTick(0.1);

        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Alarm));

        sensor.Matrix = TomMåtte(); // fjerner alt tryk
        logic.RunStateMachineTick(0.1); // spanden falder under 5000 -> skifter øjeblikkeligt tilbage

        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Monitorering));
    }

    // Test 4: Alarm på hel kolonne uden at påvirke nabokolonnen
    [Test]
    public void Alarm_Udloeses_Ved_Tryk_Langs_Hel_Kolonne()
    {
        var sensor = new FakeSensor(TomMåtte());
        var logic = new PressureMonitor(sensor);

        logic.RunStateMachineTick(0.1); 
        logic.RunStateMachineTick(0.1); 

        sensor.Matrix = KolonneTryk(0); // alle 4 punkter i kolonne 0 sættes til højtryk
        for (int i = 0; i < 104; i++) // alle spande i kolonne 0 når 5000 ved tick 104
            logic.RunStateMachineTick(0.1);

        var spande = logic.GetBuckets();

        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Alarm));
        Assert.That(spande[0, 0], Is.EqualTo(5000.0)); // alle 4 punkter i kolonne 0 fyldt op
        Assert.That(spande[1, 0], Is.EqualTo(5000.0));
        Assert.That(spande[2, 0], Is.EqualTo(5000.0));
        Assert.That(spande[3, 0], Is.EqualTo(5000.0));
        Assert.That(spande[0, 1], Is.EqualTo(0.0)); // kolonne 1 er upåvirket
    }

    // Hjælpemetoder der returnerer testmatricer til de forskellige scenarier
    private int[,] TomMåtte()
    {
        var m = new int[4, 4];
        for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                m[r, c] = 1; // ADC low — ingen belastning
        return m;
    }

    private int[,] HojtTryk()
    {
        var m = TomMåtte();
        m[0, 0] = 112; // ADC high
        return m;
    }

    private int[,] KolonneTryk(int kolonne)
    {
        var m = TomMåtte();
        for (int r = 0; r < 4; r++)
            m[r, kolonne] = 112; // ADC high - høj belastning på en hel kolonne
        return m;
    }
}
