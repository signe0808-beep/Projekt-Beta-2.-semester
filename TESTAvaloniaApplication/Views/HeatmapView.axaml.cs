using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Presentation.ViewModels;

namespace Presentation;

//partial klassen er delt i to filer: xaml og xaml.cs

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        //opretter en sensor
        var sensor = new FakeSensor(); //skiftes til den rigtige hardware sensor
        var logic = new PressureLogic2(sensor); //opretter statemachine kalder fra businesslayer
        logic.StartSystem(); //starter system kalder fra businesslayer

        //binder UI til ViewModel som indeholder data og logik til heatmap
        DataContext = new HeatmapViewModel(logic);
    }
}