using DataAccess.Interfaces;
using Iot.Device.Adc; // Her bruger vi Iot.Device.Bindings
using System;
using System.Device.Gpio;
using System.Device.Spi;
using DataAccess.Interfaces;

//Jeg har downloadet de pakker vi skal bruge for at kunne snakke sammen med RPi og AD converteren
//Logikken er det som HW snakkede om, med at tænde strøm for 1 række ad gangen og læse hver kolonne.
namespace TESTAvaloniaApplication.DataAccess.Drivers
{
    // Implementerer ISensorReader til datalaget
    //IDisposable er et interface, der sikrer, at systemet rydder op
    //og lukker de fysiske hardware-forbindelser sikkert ned,
    //når programmet er færdig med at bruge dem,
    //så benene på Raspberry Pi'en ikke bliver efterladt tændte eller låste.
    public class HardwareMatrixReader : ISensorReader, IDisposable
    {
        private GpioController _gpio;
        private SpiDevice _spiDevice;
        private Mcp3008 _mcp;

        // BCM pin-numre på Raspberry Pi, som styrer strømmen til måttens 4 rækker.
        // Skal matche den fysiske opsætning.
        private readonly int[] _rowPins = { 17, 27, 22, 23 };

        public HardwareMatrixReader()
        {
            // Initialisering af GPIO-pins (Rækker)
            _gpio = new GpioController();
            foreach (var pin in _rowPins)
            {
                // ÆNDRING 1: Start altid rækker som Input (frakoblet/High-Z) fremfor Output/Low
                _gpio.OpenPin(pin, PinMode.Input);
            }

            // Gør SPI-forbindelsen klar til MCP3008
            // BusId 0 og ChipSelectLine 0 er standard hardware-SPI på Raspberry Pi.
            var spiConnectionSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 1000000, // 1 MHz skal evt. ændres
                Mode = SpiMode.Mode0 //dette er indbygget enum. 0 betyder at vi indstiller hvornår de skal læse clocksignalet
            };

            _spiDevice = SpiDevice.Create(spiConnectionSettings);

            // Fortæller C# at det er en MCP3008 der sidder for enden af SPI-kablet
            _mcp = new Mcp3008(_spiDevice);
        }

        public int[,] ReadMatrix()
        {
            int[,] matrix = new int[4, 4];

            // Kør alle 4 rækker igennem en ad gangen
            for (int r = 0; r < 4; r++)
            {
                // ÆNDRING 2: Gør den aktuelle række til Output, før den sættes High
                _gpio.SetPinMode(_rowPins[r], PinMode.Output);
                _gpio.Write(_rowPins[r], PinValue.High);

                // Kort delay (1 ms) sikrer, at spændingen stabiliserer sig før aflæsning
                // NOTE: Hvis Velostaten er sløv til at reagere, kan I prøve at sætte denne til 5 eller 10 ms.
                System.Threading.Thread.Sleep(1);

                // Aflæser de 4 kolonner via AD-konverteren
                matrix[r, 0] = _mcp.Read(0);
                matrix[r, 1] = _mcp.Read(1);
                matrix[r, 2] = _mcp.Read(2);
                matrix[r, 3] = _mcp.Read(3);

                // ÆNDRING 3: I stedet for at sætte rækken til Low, sætter vi den tilbage til Input (frakoblet)
                _gpio.SetPinMode(_rowPins[r], PinMode.Input);
            }

            return matrix;
        }

        // Denne metode bliver automatisk kaldt, hvis programmet crasher eller lukkes ned,
        // så vi ikke efterlader Raspberry Pi'ens ben tændt eller låst.
        public void Dispose()
        {
            _mcp?.Dispose();
            _spiDevice?.Dispose();
            _gpio?.Dispose();
        }
    }
}