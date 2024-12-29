using System.Windows;
using CryptoTradingDashboard.ViewModels;

namespace CryptoTradingDashboard
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