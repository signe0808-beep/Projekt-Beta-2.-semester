using System;
using System.Security.Claims;
using Avalonia;
using DataAccess.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.ViewModels;
using Presentation.Views;
using TESTAvaloniaApplication.BusinessLayer.Services;
using TESTAvaloniaApplication.DataAccess.Simulators;
using TESTAvaloniaApplication.ViewModels;
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
        {
            // ⭐ 1. Opret DI-container
            var services = new ServiceCollection();

            // ⭐ 2. Registrér dine services
            services.AddSingleton<ISensorReader, TestSimulator>();
            services.AddSingleton<PressureLogic2>();
            services.AddSingleton<HeatmapViewModel>();
            services.AddSingleton<MainWindowViewModel>();

            services.AddTransient<HeatmapView>();
            services.AddTransient<MainWindow>();

            // ⭐ 3. Byg provider
            var provider = services.BuildServiceProvider();

            // ⭐ 4. Giv provider videre til App
            return AppBuilder.Configure(() => new App(provider))
                             .UsePlatformDetect()
                             .WithInterFont()
                             .LogToTrace();
        }

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