using Avalonia.Controls;
using Presentation.ViewModels;
using TESTAvaloniaApplication.BusinessLayer.Services;
using TESTAvaloniaApplication.DataAccess.Simulators;

namespace Presentation
{
    public partial class MainWindow : Window
    {
        private PressureLogic2 _logic;
        public MainWindow()
        {
            InitializeComponent();

            var sensor = new TestSimulator();
            _logic = new PressureLogic2(sensor);



            Heatmap.DataContext = new HeatmapViewModel(_logic);

            KalibrerButton.Click += KalibrerButton_Click;
        }

        private void KalibrerButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)  // Starter state machine og målinger når man trykker på kalibrer system knappen
        {
            _logic.StartSystem();
        }
    }
}