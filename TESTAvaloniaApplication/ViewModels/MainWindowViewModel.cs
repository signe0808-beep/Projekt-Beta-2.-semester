using TESTAvaloniaApplication.BusinessLayer.Services;
using TESTAvaloniaApplication.DataLayer.Simulators;

namespace TESTAvaloniaApplication.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // Dette er constructoren. Den kører, når skærmen åbner.
        public MainWindowViewModel()
        {
            // 1. Opret den falske sensor
            var minFalskeSensor = new TestSimulator();

            // 2. Skub den ind i motoren
            var minMotor = new PressureLogic2(minFalskeSensor);

            // 3. Start systemet!
            minMotor.StartSystem();
        }
    }
}
