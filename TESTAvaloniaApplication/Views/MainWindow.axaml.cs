using Avalonia.Controls;
using TESTAvaloniaApplication.ViewModels;

namespace Presentation
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}
