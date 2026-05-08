using System;
using System.Device.Gpio;
using System.Device.Spi;
using Iot.Device.Adc; // Her bruger vi Iot.Device.Bindings
using DataAccess.Interfaces;

//Jeg har downloadet de pakker vi skal bruge for at kunne snakke sammen med RPi og AD converteren
//Dette er bare lavet med AI, tænkte bare det ville være godt med et udgangspunkt til testen mandag
//Jeg har ikke selv sat mig 100% ind i det endnu
//I sletter bare og laver andre ting hvis det er
//Logikken er det som HW snakkede om, med at tænde strøm for 1 række ad gangen og læse være kolonne.
namespace TESTAvaloniaApplication.DataAccess.Drivers
{
    // Implementerer ISensorReader til datalaget
    //IDisposable er et interfcae, der sikrer, at systemet rydder op
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
                _gpio.OpenPin(pin, PinMode.Output);
                _gpio.Write(pin, PinValue.Low); //Sætter pin til Low (slukket) som udgangspunkt
            }

            // Gør SPI-forbindelsen klar til MCP3008
            // BusId 0 og ChipSelectLine 0 er standard hardware-SPI på Raspberry Pi.
            var spiConnectionSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 1000000, // 1 MHz skal evt. ændres
                Mode = SpiMode.Mode0
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
                // Sætter den aktuelle række til High (sender strøm igennem Velostat-materialet)
                _gpio.Write(_rowPins[r], PinValue.High);

                // Kort delay (1 ms) sikrer, at spændingen stabiliserer sig før aflæsning
                System.Threading.Thread.Sleep(1);

                // Aflæser de 4 kolonner via AD-konverteren
                // Det forudsættes, at kolonnerne er tilsluttet kanal 0, 1, 2 og 3 på MCP3008
                matrix[r, 0] = _mcp.Read(0);
                matrix[r, 1] = _mcp.Read(1);
                matrix[r, 2] = _mcp.Read(2);
                matrix[r, 3] = _mcp.Read(3);

                // Sætter rækken til Low (slukker strømmen), inden næste iteration
                _gpio.Write(_rowPins[r], PinValue.Low);
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