using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class KDJIndicator
    {
        private readonly int _period;
        private readonly int _signalPeriod;
        private readonly int _slowPeriod;

        public KDJIndicator(int period = 9, int signalPeriod = 3, int slowPeriod = 3)
        {
            _period = period;
            _signalPeriod = signalPeriod;
            _slowPeriod = slowPeriod;
        }

        public class KDJResult
        {
            public DateTime Time { get; set; }
            public decimal K { get; set; }
            public decimal D { get; set; }
            public decimal J { get; set; }
            public SignalType Signal { get; set; }
            public bool IsOverbought => K > 80;
            public bool IsOversold => K < 20;
            public bool IsBullishCrossover => K > D && PreviousK <= PreviousD;
            public bool IsBearishCrossover => K < D && PreviousK >= PreviousD;
            public decimal PreviousK { get; set; }
            public decimal PreviousD { get; set; }
        }

        public enum SignalType
        {
            Buy,
            Sell,
            Hold
        }

        public List<KDJResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<KDJResult>();
            if (candles.Count < _period) return results;

            // RSV (Raw Stochastic Value) hesaplama
            var rsvValues = CalculateRSV(candles);

            // K, D ve J değerlerini hesapla
            decimal previousK = 50;  // Başlangıç değeri
            decimal previousD = 50;  // Başlangıç değeri

            for (int i = 0; i < rsvValues.Count; i++)
            {
                decimal k = (2.0m / 3.0m * previousK) + (1.0m / 3.0m * rsvValues[i]);
                decimal d = (2.0m / 3.0m * previousD) + (1.0m / 3.0m * k);
                decimal j = 3.0m * k - 2.0m * d;

                var currentIndex = i + _period;
                results.Add(new KDJResult
                {
                    Time = candles[currentIndex].CloseTime,
                    K = k,
                    D = d,
                    J = j,
                    Signal = DetermineSignal(k, d, previousK, previousD),
                    PreviousK = previousK,
                    PreviousD = previousD
                });

                previousK = k;
                previousD = d;
            }

            return results;
        }

        private List<decimal> CalculateRSV(List<BaseCandle> candles)
        {
            var rsvValues = new List<decimal>();

            for (int i = _period - 1; i < candles.Count; i++)
            {
                var periodCandles = candles.Skip(i - _period + 1).Take(_period);
                decimal highest = periodCandles.Max(c => c.High);
                decimal lowest = periodCandles.Min(c => c.Low);
                decimal close = candles[i].Close;

                decimal rsv = (highest - lowest) == 0 ? 50 :
                    ((close - lowest) / (highest - lowest)) * 100;

                rsvValues.Add(rsv);
            }

            return rsvValues;
        }

        private SignalType DetermineSignal(decimal k, decimal d, decimal previousK, decimal previousD)
        {
            if (k > d && previousK <= previousD)
                return SignalType.Buy;
            if (k < d && previousK >= previousD)
                return SignalType.Sell;
            return SignalType.Hold;
        }

        public KDJAnalysis AnalyzeIndicator(List<KDJResult> results, int lookback = 10)
        {
            if (results.Count < lookback)
                return new KDJAnalysis { IsValid = false };

            var recentResults = results.TakeLast(lookback).ToList();

            return new KDJAnalysis
            {
                IsValid = true,
                Strength = CalculateStrength(recentResults),
                Reliability = CalculateReliability(recentResults),
                TrendStrength = CalculateTrendStrength(recentResults),
                CrossoverQuality = CalculateCrossoverQuality(recentResults)
            };
        }

        public class KDJAnalysis
        {
            public bool IsValid { get; set; }
            public decimal Strength { get; set; }
            public decimal Reliability { get; set; }
            public decimal TrendStrength { get; set; }
            public decimal CrossoverQuality { get; set; }
        }

        private decimal CalculateStrength(List<KDJResult> results)
        {
            return results.Average(r => Math.Abs(r.K - 50));
        }

        private decimal CalculateReliability(List<KDJResult> results)
        {
            int validSignals = 0;
            for (int i = 1; i < results.Count; i++)
            {
                if ((results[i].Signal == SignalType.Buy && results[i].K > results[i].PreviousK) ||
                    (results[i].Signal == SignalType.Sell && results[i].K < results[i].PreviousK))
                {
                    validSignals++;
                }
            }
            return results.Count > 1 ? (decimal)validSignals / (results.Count - 1) : 0;
        }

        private decimal CalculateTrendStrength(List<KDJResult> results)
        {
            var kTrend = results.Last().K - results.First().K;
            var dTrend = results.Last().D - results.First().D;
            return Math.Abs(kTrend + dTrend) / 2;
        }

        private decimal CalculateCrossoverQuality(List<KDJResult> results)
        {
            decimal totalCrossoverStrength = 0;
            int crossoverCount = 0;

            for (int i = 1; i < results.Count; i++)
            {
                if (results[i].IsBullishCrossover || results[i].IsBearishCrossover)
                {
                    totalCrossoverStrength += Math.Abs(results[i].K - results[i].D);
                    crossoverCount++;
                }
            }

            return crossoverCount > 0 ? totalCrossoverStrength / crossoverCount : 0;
        }
    }
}
