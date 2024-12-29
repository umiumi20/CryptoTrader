using System.Windows.Controls;

namespace CryptoTrader.Components.TradingViewPanel
{
    public partial class TradingViewPanel : UserControl
    {
        public TradingViewPanel()
        {
            InitializeComponent();
            DataContext = new ViewModels.MainViewModel();
        }
    }
}
