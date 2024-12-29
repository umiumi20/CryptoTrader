using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Trend
{
    public class ParabolicSAR
    {
        private readonly decimal _accelerationFactor;
        private readonly decimal _accelerationStep;
        private readonly decimal _maxAcceleration;

        public ParabolicSAR(decimal accelerationFactor = 0.02m, 
                           decimal accelerationStep = 0.02m, 
                           decimal maxAcceleration = 0.2m)
        {
            _accelerationFactor = accelerationFactor;
            _accelerationStep = accelerationStep;
            _maxAcceleration = maxAcceleration;
        }

        public class SARResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public bool IsUpTrend { get; set; }
            public decimal EP { get; set; }  // Extreme Point
            public decimal AF { get; set; }  // Acceleration Factor
            public SignalType Signal { get; set; }
            public decimal StopLossLevel { get; set; }
        }

        public enum SignalType { Buy, Sell, Hold }

        public List<SARResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<SARResult>();
            if (candles.Count < 2) return results;

            // lk trend yönünü belirle
            bool isUpTrend = candles[1].Close > candles[0].Close;
            decimal currentSAR = isUpTrend ? candles[0].Low : candles[0].High;
            decimal ep = isUpTrend ? candles[0].High : candles[0].Low;
            decimal af = _accelerationFactor;

            for (int i = 1; i < candles.Count; i++)
            {
                var candle = candles[i];
                decimal prevSAR = currentSAR;

                // SAR değerini hesapla
                currentSAR = prevSAR + af * (ep - prevSAR);

                // SAR değerini sınırla
                if (isUpTrend)
                {
                    currentSAR = Math.Min(currentSAR, 
                                        Math.Min(candles[i - 1].Low, 
                                               i > 1 ? candles[i - 2].Low : candles[i - 1].Low));
                    
                    if (candle.High > ep)
                    {
                        ep = candle.High;
                        af = Math.Min(af + _accelerationStep, _maxAcceleration);
                    }
                }
                else
                {
                    currentSAR = Math.Max(currentSAR, 
                                        Math.Max(candles[i - 1].High, 
                                               i > 1 ? candles[i - 2].High : candles[i - 1].High));
                    
                    if (candle.Low < ep)
                    {
                        ep = candle.Low;
                        af = Math.Min(af + _accelerationStep, _maxAcceleration);
                    }
                }

                // Trend değişimi kontrol
                bool newTrend = (isUpTrend && candle.Low < currentSAR) || 
                               (!isUpTrend && candle.High > currentSAR);

                if (newTrend)
                {
                    isUpTrend = !isUpTrend;
                    af = _accelerationFactor;
                    ep = isUpTrend ? candle.High : candle.Low;
                    currentSAR = ep;
                }

                // Stop loss seviyesini hesapla
                decimal stopLoss = isUpTrend ? 
                    Math.Max(currentSAR, candle.Low - (candle.High - candle.Low)) :
                    Math.Min(currentSAR, candle.High + (candle.High - candle.Low));

                results.Add(new SARResult
                {
                    Time = candle.CloseTime,
                    Value = currentSAR,
                    IsUpTrend = isUpTrend,
                    EP = ep,
                    AF = af,
                    Signal = DetermineSignal(isUpTrend, candle.Close, currentSAR),
                    StopLossLevel = stopLoss
                });
            }

            return results;
        }

        private SignalType DetermineSignal(bool isUpTrend, decimal close, decimal sar)
        {
            if (isUpTrend && close > sar)
                return SignalType.Buy;
            else if (!isUpTrend && close < sar)
                return SignalType.Sell;
            return SignalType.Hold;
        }

        public TrendAnalysis AnalyzeTrend(List<SARResult> results, int lookback = 10)
        {
            if (results.Count < lookback)
                return new TrendAnalysis { IsValid = false };

            var recentResults = results.TakeLast(lookback).ToList();

            return new TrendAnalysis
            {
                IsValid = true,
                TrendStrength = CalculateTrendStrength(recentResults),
                TrendDuration = CalculateTrendDuration(recentResults),
                ReversalProbability = CalculateReversalProbability(recentResults),
                StopLossDistance = CalculateAverageStopLossDistance(recentResults)
            };
        }

        public class TrendAnalysis
        {
            public bool IsValid { get; set; }
            public decimal TrendStrength { get; set; }
            public int TrendDuration { get; set; }
            public decimal ReversalProbability { get; set; }
            public decimal StopLossDistance { get; set; }
        }

        private decimal CalculateTrendStrength(List<SARResult> results)
        {
            int upCount = results.Count(r => r.IsUpTrend);
            int downCount = results.Count - upCount;
            return Math.Abs((decimal)(upCount - downCount) / results.Count);
        }

        private int CalculateTrendDuration(List<SARResult> results)
        {
            int duration = 1;
            bool currentTrend = results.Last().IsUpTrend;

            for (int i = results.Count - 2; i >= 0; i--)
            {
                if (results[i].IsUpTrend == currentTrend)
                    duration++;
                else
                    break;
            }

            return duration;
        }

        private decimal CalculateReversalProbability(List<SARResult> results)
        {
            int reversals = 0;
            for (int i = 1; i < results.Count; i++)
            {
                if (results[i].IsUpTrend != results[i - 1].IsUpTrend)
                    reversals++;
            }

            return (decimal)reversals / (results.Count - 1);
        }

        private decimal CalculateAverageStopLossDistance(List<SARResult> results)
        {
            return results.Average(r => Math.Abs(r.Value - r.StopLossLevel));
        }
    }
}
