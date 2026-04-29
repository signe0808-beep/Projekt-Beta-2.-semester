using Avalonia;
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
*/