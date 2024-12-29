using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace CryptoTrader.ViewModels
{
    public class BacktestViewModel : INotifyPropertyChanged
    {
        private string _currentDateTime = "Current Date and Time (UTC): 2024-12-28 22:13:50";
        private string _currentUser = "Current User's Login: umiumi20";
        private string _selectedTimeframe = "1h";
        private string _selectedStrategy = "EMA Cross";
        private decimal _initialBalance = 10000;
        private DateTime _startDate = DateTime.Parse("2024-01-01");
        private DateTime _endDate = DateTime.Parse("2024-12-28");
        private decimal _totalProfit = 0;
        private decimal _winRate = 0;
        private int _totalTrades = 0;
        private decimal _sharpeRatio = 0;
        private decimal _maxDrawdown = 0;

        // TimeFrames ve Strategies koleksiyonları
        public ObservableCollection<string> TimeFrames { get; } = new ObservableCollection<string>
        {
            "1m", "5m", "15m", "30m", "1h", "4h", "1d"
        };

        public ObservableCollection<string> Strategies { get; } = new ObservableCollection<string>
        {
            "EMA Cross", "RSI Strategy", "MACD Strategy", "Bollinger Bands"
        };

        // Property'ler
        public string CurrentDateTime
        {
            get => _currentDateTime;
            set
            {
                if (_currentDateTime != value)
                {
                    _currentDateTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentUser
        {
            get => _currentUser;
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedTimeframe
        {
            get => _selectedTimeframe;
            set
            {
                if (_selectedTimeframe != value)
                {
                    _selectedTimeframe = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedStrategy
        {
            get => _selectedStrategy;
            set
            {
                if (_selectedStrategy != value)
                {
                    _selectedStrategy = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal InitialBalance
        {
            get => _initialBalance;
            set
            {
                if (_initialBalance != value)
                {
                    _initialBalance = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged();
                }
            }
        }

        // Backtest sonuç property'leri
        public decimal TotalProfit
        {
            get => _totalProfit;
            set
            {
                if (_totalProfit != value)
                {
                    _totalProfit = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal WinRate
        {
            get => _winRate;
            set
            {
                if (_winRate != value)
                {
                    _winRate = value;
                    OnPropertyChanged();
                }
            }
        }

        public int TotalTrades
        {
            get => _totalTrades;
            set
            {
                if (_totalTrades != value)
                {
                    _totalTrades = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal SharpeRatio
        {
            get => _sharpeRatio;
            set
            {
                if (_sharpeRatio != value)
                {
                    _sharpeRatio = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal MaxDrawdown
        {
            get => _maxDrawdown;
            set
            {
                if (_maxDrawdown != value)
                {
                    _maxDrawdown = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
