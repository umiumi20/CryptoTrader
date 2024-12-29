using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CryptoTrader.Pilot.ViewModels
{
    public class CryptoPairViewModel : INotifyPropertyChanged
    {
        private string _symbol = string.Empty;
        private string _price = string.Empty;
        private string _change = string.Empty;
        private string _volume24h = string.Empty;
        private string _high24h = string.Empty;
        private string _low24h = string.Empty;
        private string _bidPrice = string.Empty;
        private string _askPrice = string.Empty;
        private bool _isPriceUp;

        public bool IsPriceUp
        {
            get => _isPriceUp;
            set { _isPriceUp = value; OnPropertyChanged(); }
        }

        public string Symbol
        {
            get => _symbol;
            set { _symbol = value; OnPropertyChanged(); }
        }

        public string Price
        {
            get => _price;
            set { _price = value; OnPropertyChanged(); }
        }

        public string Change
        {
            get => _change;
            set { _change = value; OnPropertyChanged(); }
        }

        public string Volume24h
        {
            get => _volume24h;
            set { _volume24h = value; OnPropertyChanged(); }
        }

        public string High24h
        {
            get => _high24h;
            set { _high24h = value; OnPropertyChanged(); }
        }

        public string Low24h
        {
            get => _low24h;
            set { _low24h = value; OnPropertyChanged(); }
        }

        public string BidPrice
        {
            get => _bidPrice;
            set { _bidPrice = value; OnPropertyChanged(); }
        }

        public string AskPrice
        {
            get => _askPrice;
            set { _askPrice = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
