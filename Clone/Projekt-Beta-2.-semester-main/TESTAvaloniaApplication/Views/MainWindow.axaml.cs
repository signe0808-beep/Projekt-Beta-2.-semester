using Avalonia.Controls;
using Presentation.ViewModels;
using TESTAvaloniaApplication.BusinessLayer.Services;
using TESTAvaloniaApplication.DataAccess.Simulators;
using System;

namespace Presentation.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var sensor = new TestSimulator(); //eller hardware data
        var logic = new PressureLogic2(sensor);

        DataContext = new HeatmapViewModel(logic);
    }
}