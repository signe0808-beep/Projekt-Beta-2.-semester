using TESTAvaloniaApplication.BusinessLayer.Services;
using TESTAvaloniaApplication.DataAccess.Simulators;
using TESTAvaloniaApplication.BusinessLayer.Interfaces;

namespace TESTAvaloniaApplication.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        // Dette er constructoren. Den kører, når skærmen åbner.
        // 1. Opret variablen HERUDE, så den overlever!
        private PressureLogic2 _minMotor;

        public MainWindowViewModel()
        {
            var minFalskeSensor = new TestSimulator();

            // 2. Gem motoren i vores nye, sikre variabel (fjern ordet 'var')
            _minMotor = new PressureLogic2(minFalskeSensor);

            // 3. Start motoren
            _minMotor.StartSystem();
        }
    }
}
