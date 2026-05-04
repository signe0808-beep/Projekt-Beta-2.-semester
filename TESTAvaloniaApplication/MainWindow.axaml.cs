using Avalonia.Controls;
using Presentation.ViewModels;
using TESTAvaloniaApplication.BusinessLayer.Services;
using TESTAvaloniaApplication.DataAccess.Simulators;

namespace Presentation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var sensor = new TestSimulator();
            var logic = new PressureLogic2(sensor);
            logic.StartSystem();

            DataContext = new HeatmapViewModel(logic);
        }
    }
}