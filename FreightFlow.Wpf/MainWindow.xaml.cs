using System.Windows;
using FreightFlow.Wpf.ViewModels;

namespace FreightFlow.Wpf
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
