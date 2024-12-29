using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class MomentumIndicator
    {
        private readonly int _period;
        private readonly decimal _threshold;

        public MomentumIndicator(int period = 14, decimal threshold = 100m)
        {
            _period = period;
            _threshold = threshold;
        }

        public class MomentumResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal NormalizedValue { get; set; }
            public SignalType Signal { get; set; }
            public decimal RateOfChange { get; set; }
            public bool IsStrong => Math.Abs(Value - 100) > _threshold;
            private readonly decimal _threshold;

            public MomentumResult(decimal threshold)
            {
                _threshold = threshold;
            }
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<MomentumResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<MomentumResult>();
            if (candles.Count <= _period) return results;

            for (int i = _period; i < candles.Count; i++)
            {
                decimal currentPrice = candles[i].Close;
                decimal previousPrice = candles[i - _period].Close;
                decimal momentumValue = (currentPrice / previousPrice) * 100m;
                decimal rateOfChange = ((currentPrice - previousPrice) / previousPrice) * 100m;

                var previousResult = results.LastOrDefault();
                
                results.Add(new MomentumResult(_threshold)
                {
                    Time = candles[i].CloseTime,
                    Value = momentumValue,
                    NormalizedValue = momentumValue - 100m,
                    RateOfChange = rateOfChange,
                    Signal = DetermineSignal(momentumValue, previousResult?.Value ?? 100m)
                });
            }

            return results;
        }

        private SignalType DetermineSignal(decimal currentValue, decimal previousValue)
        {
            if (currentValue > previousValue && currentValue > 100m)
                return SignalType.Buy;
            if (currentValue < previousValue && currentValue < 100m)
                return SignalType.Sell;
            return SignalType.Neutral;
        }

        // Momentum analizi
        public MomentumAnalysis AnalyzeMomentum(List<MomentumResult> results, int lookback = 10)
        {
            if (results.Count < lookback)
                return new MomentumAnalysis { IsValid = false };

            var recentResults = results.TakeLast(lookback).ToList();

            return new MomentumAnalysis
            {
                IsValid = true,
                Strength = CalculateStrength(recentResults),
                Persistence = CalculatePersistence(recentResults),
                Acceleration = CalculateAcceleration(recentResults),
                TrendConsistency = CalculateTrendConsistency(recentResults)
            };
        }

        public class MomentumAnalysis
        {
            public bool IsValid { get; set; }
            public decimal Strength { get; set; }
            public decimal Persistence { get; set; }
            public decimal Acceleration { get; set; }
            public decimal TrendConsistency { get; set; }
        }

        private decimal CalculateStrength(List<MomentumResult> results)
        {
            return results.Average(r => Math.Abs(r.NormalizedValue));
        }

        private decimal CalculatePersistence(List<MomentumResult> results)
        {
            int persistentCount = 0;
            for (int i = 1; i < results.Count; i++)
            {
                if ((results[i].Value > 100m && results[i-1].Value > 100m) ||
                    (results[i].Value < 100m && results[i-1].Value < 100m))
                {
                    persistentCount++;
                }
            }
            return (decimal)persistentCount / (results.Count - 1);
        }

        private decimal CalculateAcceleration(List<MomentumResult> results)
        {
            if (results.Count < 3) return 0;
            
            var changes = new List<decimal>();
            for (int i = 1; i < results.Count; i++)
            {
                changes.Add(results[i].Value - results[i-1].Value);
            }

            return changes.Average();
        }

        private decimal CalculateTrendConsistency(List<MomentumResult> results)
        {
            int consistentCount = 0;
            var values = results.Select(r => r.Value).ToList();

            for (int i = 2; i < values.Count; i++)
            {
                bool currentTrend = values[i] > values[i-1];
                bool previousTrend = values[i-1] > values[i-2];

                if (currentTrend == previousTrend)
                    consistentCount++;
            }

            return values.Count <= 2 ? 0 : (decimal)consistentCount / (values.Count - 2);
        }
    }
}
