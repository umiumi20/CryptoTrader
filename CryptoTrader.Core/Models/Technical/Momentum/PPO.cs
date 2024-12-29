using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class PPO
    {
        private readonly int _fastPeriod;
        private readonly int _slowPeriod;
        private readonly int _signalPeriod;

        public PPO(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            _fastPeriod = fastPeriod;
            _slowPeriod = slowPeriod;
            _signalPeriod = signalPeriod;
        }

        public class PPOResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal Signal { get; set; }
            public decimal Histogram { get; set; }
            public SignalType TradingSignal { get; set; }
            public bool IsBullish => Histogram > 0;
            public bool IsDiverging => Math.Abs(Histogram) > Math.Abs(PreviousHistogram);
            public decimal PreviousHistogram { get; set; }
        }

        public enum SignalType
        {
            Buy,
            Sell,
            Hold
        }

        public List<PPOResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<PPOResult>();
            if (candles.Count < _slowPeriod) return results;

            var prices = candles.Select(c => c.Close).ToList();
            
            // EMA hesaplamaları
            var fastEMA = CalculateEMA(prices, _fastPeriod);
            var slowEMA = CalculateEMA(prices, _slowPeriod);

            // PPO değerleri
            var ppoValues = new List<decimal>();
            for (int i = _slowPeriod - 1; i < prices.Count; i++)
            {
                decimal ppo = ((fastEMA[i - (_slowPeriod - _fastPeriod)] - slowEMA[i]) / slowEMA[i]) * 100;
                ppoValues.Add(ppo);
            }

            // Sinyal hattı (PPO'nun EMA'sı)
            var signalLine = CalculateEMA(ppoValues, _signalPeriod);

            // Sonuçları oluştur
            for (int i = 0; i < signalLine.Count; i++)
            {
                int currentIndex = i + _slowPeriod + _signalPeriod - 1;
                decimal histogram = ppoValues[i + _signalPeriod - 1] - signalLine[i];
                decimal previousHistogram = i > 0 ? results[i - 1].Histogram : 0;

                results.Add(new PPOResult
                {
                    Time = candles[currentIndex].CloseTime,
                    Value = ppoValues[i + _signalPeriod - 1],
                    Signal = signalLine[i],
                    Histogram = histogram,
                    PreviousHistogram = previousHistogram,
                    TradingSignal = DetermineSignal(histogram, previousHistogram)
                });
            }

            return results;
        }

        private List<decimal> CalculateEMA(List<decimal> prices, int period)
        {
            var ema = new List<decimal>();
            decimal multiplier = 2.0m / (period + 1);

            // lk SMA hesaplama
            decimal sum = prices.Take(period).Sum();
            ema.Add(sum / period);

            // EMA hesaplama
            for (int i = period; i < prices.Count; i++)
            {
                decimal currentEma = (prices[i] - ema.Last()) * multiplier + ema.Last();
                ema.Add(currentEma);
            }

            return ema;
        }

        private SignalType DetermineSignal(decimal currentHistogram, decimal previousHistogram)
        {
            if (currentHistogram > 0 && previousHistogram <= 0)
                return SignalType.Buy;
            if (currentHistogram < 0 && previousHistogram >= 0)
                return SignalType.Sell;
            return SignalType.Hold;
        }

        public PPOAnalysis AnalyzeMomentum(List<PPOResult> results, int lookback = 10)
        {
            if (results.Count < lookback)
                return new PPOAnalysis { IsValid = false };

            var recentResults = results.TakeLast(lookback).ToList();

            return new PPOAnalysis
            {
                IsValid = true,
                MomentumStrength = CalculateMomentumStrength(recentResults),
                SignalStrength = CalculateSignalStrength(recentResults),
                Divergence = CalculateDivergence(recentResults),
                TrendConsistency = CalculateTrendConsistency(recentResults)
            };
        }

        public class PPOAnalysis
        {
            public bool IsValid { get; set; }
            public decimal MomentumStrength { get; set; }
            public decimal SignalStrength { get; set; }
            public decimal Divergence { get; set; }
            public decimal TrendConsistency { get; set; }
        }

        private decimal CalculateMomentumStrength(List<PPOResult> results)
        {
            return results.Average(r => Math.Abs(r.Histogram));
        }

        private decimal CalculateSignalStrength(List<PPOResult> results)
        {
            return results.Average(r => Math.Abs(r.Value - r.Signal));
        }

        private decimal CalculateDivergence(List<PPOResult> results)
        {
            if (results.Count < 2) return 0;
            var divergences = new List<decimal>();
            
            for (int i = 1; i < results.Count; i++)
            {
                divergences.Add(Math.Abs(results[i].Histogram - results[i-1].Histogram));
            }
            
            return divergences.Average();
        }

        private decimal CalculateTrendConsistency(List<PPOResult> results)
        {
            int consistentCount = 0;
            for (int i = 1; i < results.Count; i++)
            {
                if ((results[i].Histogram > 0 && results[i-1].Histogram > 0) ||
                    (results[i].Histogram < 0 && results[i-1].Histogram < 0))
                {
                    consistentCount++;
                }
            }
            return (decimal)consistentCount / (results.Count - 1);
        }
    }
}
