using BusinessLayer.Services;
using TESTAvaloniaApplication.DataAccess.Simulators;
using TESTAvaloniaApplication.BusinessLayer.Interfaces;

namespace TESTAvaloniaApplication.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // Dette er constructoren. Den kører, når skærmen åbner.
        // 1. Opret variablen HERUDE, så den overlever!
        private PressureMonitor _minMotor;

        public MainWindowViewModel()
        {
            var minFalskeSensor = new TestSimulator();

            // 2. Gem motoren i vores nye, sikre variabel (fjern ordet 'var')
            _minMotor = new PressureMonitor(minFalskeSensor);

            // 3. Start motoren
            _minMotor.StartSystem();
        }
    }
}
