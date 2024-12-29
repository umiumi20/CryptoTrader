using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class Bias
    {
        private readonly int _period;
        private readonly decimal _overboughtLevel;
        private readonly decimal _oversoldLevel;

        public Bias(int period = 24, decimal overboughtLevel = 1.05m, decimal oversoldLevel = 0.95m)
        {
            _period = period;
            _overboughtLevel = overboughtLevel;
            _oversoldLevel = oversoldLevel;
        }

        public class BiasResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal MA { get; set; }
            public decimal Price { get; set; }
            public bool IsOverbought => Value >= _overboughtLevel;
            public bool IsOversold => Value <= _oversoldLevel;
            public SignalType Signal { get; set; }
            private readonly decimal _overboughtLevel;
            private readonly decimal _oversoldLevel;

            public BiasResult(decimal overbought, decimal oversold)
            {
                _overboughtLevel = overbought;
                _oversoldLevel = oversold;
            }
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<BiasResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<BiasResult>();
            if (candles.Count < _period) return results;

            // Calculate MA (Simple Moving Average)
            for (int i = _period - 1; i < candles.Count; i++)
            {
                var periodCandles = candles.Skip(i - _period + 1).Take(_period);
                decimal ma = periodCandles.Average(c => c.Close);
                decimal currentPrice = candles[i].Close;
                
                // Calculate Bias: (Current Price - MA) / MA * 100 + 100
                decimal bias = (currentPrice / ma);

                var result = new BiasResult(_overboughtLevel, _oversoldLevel)
                {
                    Time = candles[i].CloseTime,
                    Value = bias,
                    MA = ma,
                    Price = currentPrice,
                    Signal = DetermineSignal(bias)
                };

                results.Add(result);
            }

            return results;
        }

        private SignalType DetermineSignal(decimal bias)
        {
            if (bias <= _oversoldLevel) return SignalType.Buy;
            if (bias >= _overboughtLevel) return SignalType.Sell;
            return SignalType.Neutral;
        }

        // Calculate Percentage Bias
        public decimal CalculatePercentageBias(decimal price, decimal ma)
        {
            return ((price - ma) / ma) * 100;
        }
    }
}
