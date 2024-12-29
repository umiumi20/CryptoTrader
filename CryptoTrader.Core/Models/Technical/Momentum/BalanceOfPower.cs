using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class BalanceOfPower
    {
        private readonly int _smoothPeriod;
        private readonly decimal _threshold;

        public BalanceOfPower(int smoothPeriod = 14, decimal threshold = 0.5m)
        {
            _smoothPeriod = smoothPeriod;
            _threshold = threshold;
        }

        public class BOPResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal SmoothedValue { get; set; }
            public decimal BuyingPressure { get; set; }
            public decimal SellingPressure { get; set; }
            public SignalType Signal { get; set; }
            public bool IsBullish => Value > 0;
            public bool IsBearish => Value < 0;
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<BOPResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<BOPResult>();
            if (candles.Count < _smoothPeriod) return results;

            // Calculate raw BOP values
            var rawBOP = new List<decimal>();
            foreach (var candle in candles)
            {
                decimal bop = CalculateRawBOP(candle);
                rawBOP.Add(bop);
            }

            // Apply smoothing
            var smoothedBOP = CalculateSMA(rawBOP, _smoothPeriod);

            // Generate results
            for (int i = _smoothPeriod - 1; i < candles.Count; i++)
            {
                var currentBOP = rawBOP[i];
                var smoothedValue = smoothedBOP[i - _smoothPeriod + 1];
                
                decimal buyingPressure = Math.Max(currentBOP, 0);
                decimal sellingPressure = Math.Abs(Math.Min(currentBOP, 0));

                var result = new BOPResult
                {
                    Time = candles[i].CloseTime,
                    Value = currentBOP,
                    SmoothedValue = smoothedValue,
                    BuyingPressure = buyingPressure,
                    SellingPressure = sellingPressure,
                    Signal = DetermineSignal(smoothedValue)
                };

                results.Add(result);
            }

            return results;
        }

        private decimal CalculateRawBOP(BaseCandle candle)
        {
            decimal range = candle.High - candle.Low;
            if (range == 0) return 0;

            return (candle.Close - candle.Open) / range;
        }

        private List<decimal> CalculateSMA(List<decimal> values, int period)
        {
            var results = new List<decimal>();
            for (int i = period - 1; i < values.Count; i++)
            {
                var sum = values.Skip(i - period + 1).Take(period).Sum();
                results.Add(sum / period);
            }
            return results;
        }

        private SignalType DetermineSignal(decimal bop)
        {
            if (bop > _threshold) return SignalType.Buy;
            if (bop < -_threshold) return SignalType.Sell;
            return SignalType.Neutral;
        }

        // Divergence detection methods
        public bool IsBullishDivergence(List<BOPResult> bopResults, List<BaseCandle> candles, int lookback = 5)
        {
            if (bopResults.Count < lookback || candles.Count < lookback) return false;

            var recentBOP = bopResults.TakeLast(lookback).ToList();
            var recentCandles = candles.TakeLast(lookback).ToList();

            bool bopMakingHigherLows = IsHigherLows(recentBOP.Select(x => x.Value).ToList());
            bool priceMakingLowerLows = IsLowerLows(recentCandles.Select(x => x.Close).ToList());

            return bopMakingHigherLows && priceMakingLowerLows;
        }

        public bool IsBearishDivergence(List<BOPResult> bopResults, List<BaseCandle> candles, int lookback = 5)
        {
            if (bopResults.Count < lookback || candles.Count < lookback) return false;

            var recentBOP = bopResults.TakeLast(lookback).ToList();
            var recentCandles = candles.TakeLast(lookback).ToList();

            bool bopMakingLowerHighs = IsLowerHighs(recentBOP.Select(x => x.Value).ToList());
            bool priceMakingHigherHighs = IsHigherHighs(recentCandles.Select(x => x.Close).ToList());

            return bopMakingLowerHighs && priceMakingHigherHighs;
        }

        private bool IsHigherLows(List<decimal> values)
        {
            decimal minValue = decimal.MaxValue;
            bool foundHigherLow = false;

            foreach (var value in values)
            {
                if (value < minValue)
                {
                    if (minValue != decimal.MaxValue && value > values.Min())
                        foundHigherLow = true;
                    minValue = value;
                }
            }

            return foundHigherLow;
        }

        private bool IsLowerLows(List<decimal> values)
        {
            decimal minValue = decimal.MaxValue;
            bool foundLowerLow = false;

            foreach (var value in values)
            {
                if (value < minValue)
                {
                    if (minValue != decimal.MaxValue)
                        foundLowerLow = true;
                    minValue = value;
                }
            }

            return foundLowerLow;
        }

        private bool IsHigherHighs(List<decimal> values)
        {
            decimal maxValue = decimal.MinValue;
            bool foundHigherHigh = false;

            foreach (var value in values)
            {
                if (value > maxValue)
                {
                    if (maxValue != decimal.MinValue)
                        foundHigherHigh = true;
                    maxValue = value;
                }
            }

            return foundHigherHigh;
        }

        private bool IsLowerHighs(List<decimal> values)
        {
            decimal maxValue = decimal.MinValue;
            bool foundLowerHigh = false;

            foreach (var value in values)
            {
                if (value > maxValue && value < values.Max())
                {
                    foundLowerHigh = true;
                    maxValue = value;
                }
            }

            return foundLowerHigh;
        }
    }
}
