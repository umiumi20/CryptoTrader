using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class CommodityChannelIndex
    {
        private readonly int _period;
        private readonly decimal _constant;
        private readonly decimal _overboughtLevel;
        private readonly decimal _oversoldLevel;

        public CommodityChannelIndex(int period = 20, decimal constant = 0.015m, 
            decimal overboughtLevel = 100m, decimal oversoldLevel = -100m)
        {
            _period = period;
            _constant = constant;
            _overboughtLevel = overboughtLevel;
            _oversoldLevel = oversoldLevel;
        }

        public class CCIResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal TypicalPrice { get; set; }
            public decimal MA { get; set; }
            public decimal MeanDeviation { get; set; }
            public SignalType Signal { get; set; }
            public bool IsOverbought => Value >= _overboughtLevel;
            public bool IsOversold => Value <= _oversoldLevel;
            private readonly decimal _overboughtLevel;
            private readonly decimal _oversoldLevel;

            public CCIResult(decimal overbought, decimal oversold)
            {
                _overboughtLevel = overbought;
                _oversoldLevel = oversold;
            }
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<CCIResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<CCIResult>();
            if (candles.Count < _period) return results;

            // Calculate Typical Prices
            var typicalPrices = candles.Select(c => (c.High + c.Low + c.Close) / 3).ToList();

            // Calculate SMA of Typical Prices
            for (int i = _period - 1; i < candles.Count; i++)
            {
                var periodTypicalPrices = typicalPrices.Skip(i - _period + 1).Take(_period).ToList();
                decimal smaTypicalPrice = periodTypicalPrices.Average();

                // Calculate Mean Deviation
                decimal sumDeviation = 0;
                for (int j = 0; j < _period; j++)
                {
                    sumDeviation += Math.Abs(periodTypicalPrices[j] - smaTypicalPrice);
                }
                decimal meanDeviation = sumDeviation / _period;

                // Calculate CCI
                decimal currentTypicalPrice = typicalPrices[i];
                decimal cci = meanDeviation == 0 ? 0 : 
                    (currentTypicalPrice - smaTypicalPrice) / (meanDeviation * _constant);

                var result = new CCIResult(_overboughtLevel, _oversoldLevel)
                {
                    Time = candles[i].CloseTime,
                    Value = cci,
                    TypicalPrice = currentTypicalPrice,
                    MA = smaTypicalPrice,
                    MeanDeviation = meanDeviation,
                    Signal = DetermineSignal(cci)
                };

                results.Add(result);
            }

            return results;
        }

        private SignalType DetermineSignal(decimal cci)
        {
            if (cci <= _oversoldLevel) return SignalType.Buy;
            if (cci >= _overboughtLevel) return SignalType.Sell;
            return SignalType.Neutral;
        }

        // Trend Analysis Methods
        public bool IsBullishTrend(List<CCIResult> results, int lookback = 5)
        {
            if (results.Count < lookback) return false;
            var recentResults = results.TakeLast(lookback).ToList();
            return recentResults.All(r => r.Value > 0);
        }

        public bool IsBearishTrend(List<CCIResult> results, int lookback = 5)
        {
            if (results.Count < lookback) return false;
            var recentResults = results.TakeLast(lookback).ToList();
            return recentResults.All(r => r.Value < 0);
        }

        // Divergence Detection
        public bool IsBullishDivergence(List<CCIResult> results, List<BaseCandle> candles, int lookback = 5)
        {
            if (results.Count < lookback || candles.Count < lookback) return false;
            
            var recentCCI = results.TakeLast(lookback).Select(r => r.Value).ToList();
            var recentPrices = candles.TakeLast(lookback).Select(c => c.Low).ToList();

            bool cciMakingHigherLow = IsHigherLow(recentCCI);
            bool priceMakingLowerLow = IsLowerLow(recentPrices);

            return cciMakingHigherLow && priceMakingLowerLow;
        }

        public bool IsBearishDivergence(List<CCIResult> results, List<BaseCandle> candles, int lookback = 5)
        {
            if (results.Count < lookback || candles.Count < lookback) return false;
            
            var recentCCI = results.TakeLast(lookback).Select(r => r.Value).ToList();
            var recentPrices = candles.TakeLast(lookback).Select(c => c.High).ToList();

            bool cciMakingLowerHigh = IsLowerHigh(recentCCI);
            bool priceMakingHigherHigh = IsHigherHigh(recentPrices);

            return cciMakingLowerHigh && priceMakingHigherHigh;
        }

        private bool IsHigherLow(List<decimal> values)
        {
            decimal previousLow = decimal.MaxValue;
            for (int i = 0; i < values.Count - 1; i++)
            {
                if (values[i] < previousLow)
                {
                    if (previousLow != decimal.MaxValue && values[i] > values.Min())
                        return true;
                    previousLow = values[i];
                }
            }
            return false;
        }

        private bool IsLowerLow(List<decimal> values)
        {
            return values.Last() < values.Min();
        }

        private bool IsLowerHigh(List<decimal> values)
        {
            decimal previousHigh = decimal.MinValue;
            for (int i = 0; i < values.Count - 1; i++)
            {
                if (values[i] > previousHigh)
                {
                    if (previousHigh != decimal.MinValue && values[i] < values.Max())
                        return true;
                    previousHigh = values[i];
                }
            }
            return false;
        }

        private bool IsHigherHigh(List<decimal> values)
        {
            return values.Last() > values.Max();
        }
    }
}
