using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class FisherTransform
    {
        private readonly int _period;
        private readonly decimal _scale;

        public FisherTransform(int period = 10, decimal scale = 0.5m)
        {
            _period = period;
            _scale = scale;
        }

        public class FisherResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal Trigger { get; set; }
            public SignalType Signal { get; set; }
            public TrendStrength Strength { get; set; }
            public bool IsBullish => Value > Trigger;
            public bool IsCrossover => Math.Sign((double)Value) != Math.Sign((double)PreviousValue);
            public decimal PreviousValue { get; set; }
        }

        public enum SignalType
        {
            Buy,
            Sell,
            Hold
        }

        public enum TrendStrength
        {
            Strong,
            Moderate,
            Weak,
            Neutral
        }

        public List<FisherResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<FisherResult>();
            if (candles.Count < _period) return results;

            // Medyan fiyat hesapla
            var medianPrices = candles.Select(c => (c.High + c.Low) / 2m).ToList();

            // Normalize edilmiş fiyatlar
            var normalizedPrices = new List<decimal>();
            for (int i = _period - 1; i < medianPrices.Count; i++)
            {
                var periodPrices = medianPrices.Skip(i - _period + 1).Take(_period);
                var maxPrice = periodPrices.Max();
                var minPrice = periodPrices.Min();
                var currentPrice = medianPrices[i];

                decimal normalizedPrice = (maxPrice - minPrice) == 0 ? 0 :
                    ((currentPrice - minPrice) / (maxPrice - minPrice) - 0.5m) * _scale;

                normalizedPrices.Add(normalizedPrice);
            }

            // Fisher Transform uygula
            decimal previousValue = 0;
            decimal previousTrigger = 0;

            for (int i = 0; i < normalizedPrices.Count; i++)
            {
                decimal value = 0.5m * (decimal)Math.Log((double)((1 + normalizedPrices[i]) / (1 - normalizedPrices[i]))) +
                              0.5m * previousValue;
                
                decimal trigger = previousValue;

                var currentIndex = i + _period - 1;
                results.Add(new FisherResult
                {
                    Time = candles[currentIndex].CloseTime,
                    Value = value,
                    Trigger = trigger,
                    Signal = DetermineSignal(value, trigger, previousValue),
                    Strength = DetermineTrendStrength(value, trigger),
                    PreviousValue = previousValue
                });

                previousValue = value;
                previousTrigger = trigger;
            }

            return results;
        }

        private SignalType DetermineSignal(decimal value, decimal trigger, decimal previousValue)
        {
            if (value > trigger && previousValue <= trigger)
                return SignalType.Buy;
            if (value < trigger && previousValue >= trigger)
                return SignalType.Sell;
            return SignalType.Hold;
        }

        private TrendStrength DetermineTrendStrength(decimal value, decimal trigger)
        {
            decimal difference = Math.Abs(value - trigger);
            
            if (difference >= 2.0m)
                return TrendStrength.Strong;
            else if (difference >= 1.0m)
                return TrendStrength.Moderate;
            else if (difference >= 0.5m)
                return TrendStrength.Weak;
            else
                return TrendStrength.Neutral;
        }

        public FisherAnalysis Analyze(List<FisherResult> results, int lookback = 10)
        {
            if (results.Count < lookback)
                return new FisherAnalysis { IsValid = false };

            var recentResults = results.TakeLast(lookback).ToList();

            return new FisherAnalysis
            {
                IsValid = true,
                TrendStrength = CalculateTrendStrength(recentResults),
                Volatility = CalculateVolatility(recentResults),
                MomentumScore = CalculateMomentumScore(recentResults),
                SignalReliability = CalculateSignalReliability(recentResults)
            };
        }

        public class FisherAnalysis
        {
            public bool IsValid { get; set; }
            public decimal TrendStrength { get; set; }
            public decimal Volatility { get; set; }
            public decimal MomentumScore { get; set; }
            public decimal SignalReliability { get; set; }
        }

        private decimal CalculateTrendStrength(List<FisherResult> results)
        {
            return results.Average(r => Math.Abs(r.Value));
        }

        private decimal CalculateVolatility(List<FisherResult> results)
        {
            var values = results.Select(r => r.Value).ToList();
            decimal mean = values.Average();
            decimal sumSquares = values.Sum(v => (v - mean) * (v - mean));
            return (decimal)Math.Sqrt((double)(sumSquares / values.Count));
        }

        private decimal CalculateMomentumScore(List<FisherResult> results)
        {
            var momentum = results.Last().Value - results.First().Value;
            return momentum / results.Count;
        }

        private decimal CalculateSignalReliability(List<FisherResult> results)
        {
            int validSignals = 0;
            int totalSignals = 0;

            for (int i = 1; i < results.Count; i++)
            {
                if (results[i].Signal != SignalType.Hold)
                {
                    totalSignals++;
                    if ((results[i].Signal == SignalType.Buy && results[i].Value > results[i-1].Value) ||
                        (results[i].Signal == SignalType.Sell && results[i].Value < results[i-1].Value))
                    {
                        validSignals++;
                    }
                }
            }

            return totalSignals == 0 ? 0 : (decimal)validSignals / totalSignals;
        }
    }
}
