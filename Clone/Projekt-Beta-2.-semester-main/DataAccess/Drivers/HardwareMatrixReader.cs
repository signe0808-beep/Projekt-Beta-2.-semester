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
    // Vi tilføjer IDisposable så vi kan rydde pænt op i hardware-forbindelserne, når programmet lukkes
    public class HardwareMatrixReader : ISensorReader, IDisposable
    {
        private GpioController _gpio;
        private SpiDevice _spiDevice;
        private Mcp3008 _mcp;

        // BCM pin-numrene på Raspberry Pi'en, som I forbinder til måttens rækker. 
        // Ret disse tal, så de passer til de ben, hardware-holdet vælger at bruge!
        private readonly int[] _rowPins = { 17, 27, 22, 23 };

        public HardwareMatrixReader()
        {
            // 1. Gør Raspberry Pi'ens ben klar (GPIO)
            _gpio = new GpioController();
            foreach (var pin in _rowPins)
            {
                _gpio.OpenPin(pin, PinMode.Output);
                _gpio.Write(pin, PinValue.Low); // Start med at slukke for strømmen
            }

            // 2. Gør SPI-forbindelsen klar til MCP3008
            // (BusId 0 og ChipSelectLine 0 er standard hardware SPI på en Pi)
            var spiConnectionSettings = new SpiConnectionSettings(0, 0)
            {
                ClockFrequency = 1000000, // 1 MHz er hurtigt og stabilt
                Mode = SpiMode.Mode0
            };

            _spiDevice = SpiDevice.Create(spiConnectionSettings);

            // 3. Fortæl C# at det er en MCP3008 der sidder for enden af SPI-kablet
            _mcp = new Mcp3008(_spiDevice);
        }

        public int[,] ReadMatrix()
        {
            int[,] matrix = new int[4, 4];

            // Kør alle 4 rækker igennem én ad gangen
            for (int r = 0; r < 4; r++)
            {
                // TÆND for strømmen på rækken
                _gpio.Write(_rowPins[r], PinValue.High);

                // Giv hardwaren et ultra-kort øjeblik til at stabilisere spændingen (1 millisekund)
                System.Threading.Thread.Sleep(1);

                // LÆS kolonnerne via vores MCP3008 analog-til-digital konverter.
                // Vi forudsætter her, at hardware-holdet sætter kolonnerne til kanal 0, 1, 2 og 3 på chippen.
                matrix[r, 0] = _mcp.Read(0);
                matrix[r, 1] = _mcp.Read(1);
                matrix[r, 2] = _mcp.Read(2);
                matrix[r, 3] = _mcp.Read(3);

                // SLUK for rækken igen, inden vi går videre til den næste
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