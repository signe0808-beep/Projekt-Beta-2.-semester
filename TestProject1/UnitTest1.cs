using System.Runtime.Intrinsics.X86;
using BusinessLayer.Services;
using DataAccess.Interfaces;
using TESTAvaloniaApplication.BusinessLayer.Models;

namespace TestProject1;
class FakeSensor : ISensorReader //Opretter en fake sensor klasse, som implementere ISensorReader,
                                 //så pressurelogic2 tror at det her er den rigtige sensor 
{
    public int[,] Matrix { get; set; } //property som er en 4x4 matrix med sensorværdier,
                                       //som man skiftes undervejs i testen for at simulere at nogen sætter sig eller rejser sig

    public FakeSensor(int[,] matrix) //opretter contructoren til Fakesensor, som kaldes når sensoren bliver oprettet og den skal have en startmatrix. Altså det vi sender IND
    {
        Matrix = matrix; //gemmer den matrix vi sender ind i Matrix-Propertyen, så sensoren kan levere data. Det som bliver gemt og brugt
    }

    public int[,] ReadMatrix() //en metode som kaldes af Pressurelogic2 ved hvert tick for at hente sensordata
    {
        return Matrix; //returnere den matrix vi har sat i testene, så enten TomMåtte(), højttryk() eller kolonnetryk
    }
}

public class Tests
{
 // Test 1: Alarm udløses ved hårdt, vedvarende tryk på ét punkt

    [Test] //en attribut som fortæller NUnit at dette er en test som skal køres
    public void
    Alarm_Udloeses_Ved_Vedvarende_Hojt_Tryk() //metode hvor testens navn beskriver hvad den tester
    {
      var sensor = new FakeSensor(TomMåtte()); //variabel der holder fakesensor, hvilket er det der skiftes matrix på undervejs i testen for at simulere forskellige situationer af måtten.
      var logic = new PressureMonitor(sensor); //her bliver logikken for hvordan pressurelogic fungere, det er state maskine, leaky bucket og afgørelse om der skal være alarm. Logic bruger altså sensor til at hente data ved hvert tick. Sensor levere tal -> logic beregner -> alarm eller ej

      logic.RunStateMachineTick(0.1); //Her bliver Pressurelogic initialiseret går fra initialisering -> kalibrering
      logic.RunStateMachineTick(0.1); //Tick 2 går fra kalibrering -> monitorering.reference gemmes som 1000. 0.1 er delta time som hjælper systemet med at køre ordentligt.

      sensor.Matrix = HojtTryk(); //her bliver den sensor vi oprettede som property skiftet til sensoren i højtryk, der simulere højt tryk på punkt (0,0)
      for (int i = 0; i < 55; i++) //kører 55 ticks, som er nok til at spanden fyldes op til 300 på punkt (0,0)
                                    //for loopet starter tælleren på 0 og fortæstter til den er under 55, hvilket er nok til at den rammer 300 på punkt (0,0)
      logic.RunStateMachineTick(0.1); //kører et tick med 100ms (delta time) - spanden fyldes mere og mere for hver tick. Skal sikkert rettes når vi kender til de rigtige værdier

      Assert.That(logic.CurrentState,
      Is.EqualTo(SystemStateEnum.Alarm)); //sikrer at current state er gået i alarm, for at testen virker
    }

// Test 2: Ingen alarm når trykket er under NOISE_FLOOR
   [Test] //attribut
    public void Ingen_Alarm_Ved_Stoej_Under_Noise_Floor() //navn på metoden som tester det den hedder. Men i virkeligheden tester den bare at en tom måtte ikke giver alarm. Når vi ved hvor meget støj en rigtig sensor producere kan det testes nærmere.
    {
        var sensor = new FakeSensor(TomMåtte());
        var logic = new PressureMonitor(sensor);

        //sensor matrixen bliver IKKE skiftet til noget, derfor skulle uendelig mængde ticks ikke gøre en forskel. Den er bare på tom måtte. Da måtten forbliver tom skulle den aldrig nogensinde udløse en alarm.
        for (int i = 0; i < 50; i++) //tester at når der bliver kørt et tick 50 gange skulle det ikke udløse en alarm, da den er tom
            logic.RunStateMachineTick(0.1); //kører et tick på 100ms - måtten er tom så spanden burde ikke blive fyldt
        
        Assert.That(logic.CurrentState,
            Is.EqualTo(SystemStateEnum.Monitorering)); //sørg for at programmet stadig er i monitorering og ikke skifter til alarm
    }

    // Test 3: Systemet vender tilbage til Monitorering når trykket fjernes
   [Test]
      public void System_Vender_Tilbage_Til_Monitorering_Naar_Tryk_Fjernes() //navn forklare hvad testen gør
    {
        var sensor = new FakeSensor(TomMåtte()); 
        var logic = new PressureMonitor(sensor);

        logic.RunStateMachineTick(0.1); //Init -> kalibrering
        logic.RunStateMachineTick(0.1); //Kalibrering -> monitorering

          sensor.Matrix = HojtTryk();  //skifter sensoren til højtryk som simulerer at en person sætter sig
        for (int i = 0; i < 55; i++)
            logic.RunStateMachineTick(0.1); //ligesom før, skaber alarm
         
        Assert.That(logic.CurrentState,
            Is.EqualTo(SystemStateEnum.Alarm)); //særg for at den er i alarm

        sensor.Matrix = TomMåtte(); //fjerner alt tryk
        logic.RunStateMachineTick(0.1); //kører et tick, spanden falder under 300 og state maskine skifter øjeblikkeligt tilbage til monitorering

        Assert.That(logic.CurrentState,
            Is.EqualTo(SystemStateEnum.Monitorering)); //sørg for at den kommer tilbage til monitorering
    }

    // Test 4: Alarm på hel kolonne, uden at påvirke nabopunkter
    [Test]
    public void Alarm_Udloeses_Ved_Tryk_Langs_Hel_Kolonne() //navn der fortæller hvad metoden gr
    {
        var sensor = new FakeSensor(TomMåtte());
        var logic = new PressureMonitor(sensor);

        logic.RunStateMachineTick(0.1); //Init -> kalibrering
        logic.RunStateMachineTick(0.1); //Kalibrering -> monitorering
        
        sensor.Matrix = KolonneTryk(0); //bruger hjælpemetoden der skifter sensoren så alle 4 punkter i kolonne 0 har højtryk
        for (int i = 0; i < 55; i++) //kører 55 ticks, nok til at alle 4 spande i kolonne 0 fyldes op til 300
            logic.RunStateMachineTick(0.1); //fylder spanden mere og mere
        
        var spande = logic.GetBuckets(); //henter alle 16 spandeværdier fra logic så de kan tjekkes. SÅ de præcise værdier inde i alle spandene findes frem

        Assert.That(logic.CurrentState,
            Is.EqualTo(SystemStateEnum.Alarm)); //tjekker at systemet er gået i alarm

        Assert.That(spande[0, 0], Is.EqualTo(300.0)); //tjekker at de præcise punkter er fyldt op til 300
        Assert.That(spande[1, 0], Is.EqualTo(300.0));
        Assert.That(spande[2, 0], Is.EqualTo(300.0));
        Assert.That(spande[3, 0], Is.EqualTo(300.0));
        Assert.That(spande[0, 1], Is.EqualTo(0.0));  //tjekker at kolonne 1 er upåvirket, da algoritmen ikke må gå ud over naboen
    }

    //alt under her er hjælpemetoder, hvor de forskellige typer af "test" måtter returneres, så vi kan teste om programmet virker som det skal
    private int[,] TomMåtte() //Opretter en 4x4 matrix hvor alle 16 punkter er 1000 (tom måtte, ingen belastning - modstand fra velostat sensorne)
    {
        var m = new int[4, 4]; //opretter en tom 4x4 matrix
        for (int r = 0; r < 4; r++) //løber igennem alle 4 rækker
            for (int c = 0; c < 4; c++) //løber igennem alle 4 kolonner
                m[r, c] = 1000; //sætter hvert punkt i matrixen til 1000
        return m; //returnere den færdige matrix
    }

    private int[,] HojtTryk() //Opretter næste matrix  som skal simulere et højt tryk et sted på måtten
    {
        var m = TomMåtte(); //vi har allerede oprettet en tom 4x4 matrix, så den genbruger vi bare
        m[0, 0] = 200; //men vi sætter punktet 0,0 i matrixen til at være 200, da lavere værdier signalere højere tryk på måtten
        return m; //returnere måtten med et højt tryk
    }

    private int[,] KolonneTryk(int kolonne) //opretter en matrix hvor der er højt tryk i en hel kolonne for at teste at det ikke påvirker de andre punkter
    {                                        //opretter også en kolonne variabel
        var m = TomMåtte(); //genbruger den tomme måtte
        for (int r = 0; r < 4; r++) //løber igennem alle 4 rækker
            m[r, kolonne] = 200; //sætter en kolonne til at have højt tryk på 200, så man kan selv bestemme hvilken kolonne det skal være da det er typen int, og så bliver den valgte kolonne 200
        return m; //returnere måtten med den ene kolonne som har højt tryk
    }

    //boundary test

    // Boundary Test 1: 49 ticks → spand = 294.0 → ingen alarm (lige under grænsen)
  [Test]
    public void Graensevaerdi_49_Ticks_Giver_Ingen_Alarm()
    {
        var sensor = new FakeSensor(TomMåtte());
        var logic = new PressureMonitor(sensor);

        logic.RunStateMachineTick(0.1); // Init → Kalibrering
        logic.RunStateMachineTick(0.1); // Kalibrering → Monitorering

        sensor.Matrix = HojtTryk(); // raw=200 → 80% tryk → netto 6.0 pr. tick
      for (int i = 0; i < 49; i++) // 49 * 6.0 = 294.0 → under 300
            logic.RunStateMachineTick(0.1);

        Assert.That(logic.GetBuckets()[0, 0], Is.EqualTo(294.0));
        Assert.That(logic.CurrentState,
    Is.EqualTo(SystemStateEnum.Monitorering));
    }

    // Boundary Test 2: 50 ticks → spand = 300.0 → alarm (præcis på grænsen)
  [Test]
    public void Graensevaerdi_50_Ticks_Udloser_Alarm()
    {
        var sensor = new FakeSensor(TomMåtte());
        var logic = new PressureMonitor(sensor);

        logic.RunStateMachineTick(0.1);
        logic.RunStateMachineTick(0.1);

        sensor.Matrix = HojtTryk(); // raw=200 → 80% → netto 6.0 pr. tick
        for (int i = 0; i < 50; i++) // 50 * 6.0 = 300.0 → præcis på grænsen
          logic.RunStateMachineTick(0.1);

        Assert.That(logic.GetBuckets()[0, 0], Is.EqualTo(300.0));
        Assert.That(logic.CurrentState, Is.EqualTo(SystemStateEnum.Alarm));
    }

    // Boundary Test 3: raw=901 → 9.9% tryk → under noise floor → ignoreres
    [Test]
    public void Graensevaerdi_Under_NoiseFloor_Ignoreres()
    {
        var sensor = new FakeSensor(TomMåtte());
        var logic = new PressureMonitor(sensor);

        logic.RunStateMachineTick(0.1);
        logic.RunStateMachineTick(0.1);

        var m = TomMåtte();
        m[0, 0] = 901; // ((1000-901)/1000)*100 = 9.9% < 10% → filtreres fra
        sensor.Matrix = m;

        for (int i = 0; i < 100; i++)
            logic.RunStateMachineTick(0.1);

        Assert.That(logic.GetBuckets()[0, 0], Is.EqualTo(0.0));
        Assert.That(logic.CurrentState,
    Is.EqualTo(SystemStateEnum.Monitorering));
    }
}
