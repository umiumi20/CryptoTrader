using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CryptoTrader.Core.Models.Candlestick;
using CryptoTrader.Core.Models.Technical;

namespace CryptoTrader.Pilot.ViewModels
{
    public class CandleViewModel : INotifyPropertyChanged
    {
        private BaseCandle _candle;
        private List<IndicatorValue> _indicators;

        public CandleViewModel(BaseCandle candle)
        {
            _candle = candle;
            _indicators = new List<IndicatorValue>();
        }

        public BaseCandle Candle
        {
            get => _candle;
            set
            {
                if (_candle != value)
                {
                    _candle = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<IndicatorValue> Indicators
        {
            get => _indicators;
            set
            {
                if (_indicators != value)
                {
                    _indicators = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
