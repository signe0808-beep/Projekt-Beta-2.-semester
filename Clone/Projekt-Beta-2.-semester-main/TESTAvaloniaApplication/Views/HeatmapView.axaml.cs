using Avalonia.Controls;
using Presentation.ViewModels;
using TESTAvaloniaApplication.BusinessLayer.Services;


namespace Presentation;

public partial class HeatmapView : UserControl
{
    public HeatmapView(HeatmapViewModel vm)
    {
        InitializeComponent();
        
        DataContext = vm;
    }
}
