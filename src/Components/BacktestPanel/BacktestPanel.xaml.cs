using System.Windows.Controls;
using CryptoTrader.ViewModels;

namespace CryptoTrader.Components.BacktestPanel
{
    public partial class BacktestPanel : UserControl
    {
        private BacktestViewModel _viewModel;

        public BacktestPanel()
        {
            InitializeComponent();
            _viewModel = new BacktestViewModel();
            DataContext = _viewModel;
        }
    }
}
