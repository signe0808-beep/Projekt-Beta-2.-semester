using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using System.Linq;
using TESTAvaloniaApplication.ViewModels;
using Presentation;
using DataAccess.Interfaces;
using TESTAvaloniaApplication.DataAccess.Simulators;
using BusinessLayer.Services;
using TESTAvaloniaApplication.BusinessLayer.Interfaces;
using TESTAvaloniaApplication.DataAccess.Drivers;


namespace Presentation.Views
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                //skift herinde til HardwareMatrixReader
                ISensorReader minSensor = new HardwareMatrixReader();

                //bygger forretningslaget:
                IPressureMonitor minMotor = new PressureMonitor(minSensor);
                //bygger præsentationslaget;
                var minViewModel = new MainWindowViewModel(minMotor);

                desktop.MainWindow = new MainWindow()
                {
                    DataContext = minViewModel
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }
    }
}