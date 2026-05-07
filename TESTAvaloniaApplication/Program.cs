using Avalonia;
using Presentation.Views;
using System;
using System.Security.Claims;
using TESTAvaloniaApplication.DataAccess.Simulators;
using static System.Net.Mime.MediaTypeNames;

namespace TESTAvaloniaApplication
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();

    }


   
}
/*
Main() {

    ts = new TestSimulator();
    pl = new PressureLogic(ts);

    pl.RunStateMachineTick();

    if (pl.CurrentSTate == Alarm)
        WriteLine(Test1 gik godt)
    else
        WriteLine(Test1 eksploderede)
}

  using System;
  using System.Device.Gpio;
  using System.Device.Spi;
  using Iot.Device.Adc;

  var gpio = new GpioController();
  int rowPin = 17; // skift til det rigtige pin nummer

  gpio.OpenPin(rowPin, PinMode.Output);
  gpio.Write(rowPin, PinValue.Low);

  var spi = SpiDevice.Create(new SpiConnectionSettings(0, 0)
  {
      ClockFrequency = 1000000,
      Mode = SpiMode.Mode0
  });

  var mcp = new Mcp3008(spi);

  while (true)
  {
      gpio.Write(rowPin, PinValue.High);
      Thread.Sleep(1);
      Console.WriteLine($"Kanal 0: {mcp.Read(0)} | Kanal 1: {mcp.Read(1)} | Kanal
  2: {mcp.Read(2)} | Kanal 3: {mcp.Read(3)}");
      gpio.Write(rowPin, PinValue.Low);
      Thread.Sleep(500);
  }



*/

