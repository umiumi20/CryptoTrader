using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class MoneyFlowIndex
    {
        private readonly int _period;
        private readonly decimal _overboughtLevel;
        private readonly decimal _oversoldLevel;

        public MoneyFlowIndex(int period = 14, decimal overboughtLevel = 80m, decimal oversoldLevel = 20m)
        {
            _period = period;
            _overboughtLevel = overboughtLevel;
            _oversoldLevel = oversoldLevel;
        }

        public class MFIResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal PositiveFlow { get; set; }
            public decimal NegativeFlow { get; set; }
            public decimal MoneyRatio { get; set; }
            public SignalType Signal { get; set; }
            public bool IsOverbought => Value >= _overboughtLevel;
            public bool IsOversold => Value <= _oversoldLevel;
            private readonly decimal _overboughtLevel;
            private readonly decimal _oversoldLevel;

            public MFIResult(decimal overbought, decimal oversold)
            {
                _overboughtLevel = overbought;
                _oversoldLevel = oversold;
            }
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<MFIResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<MFIResult>();
            if (candles.Count <= _period) return results;

            // Tipik fiyat hesapla
            var typicalPrices = candles.Select(c => (c.High + c.Low + c.Close) / 3).ToList();

            // Raw Money Flow hesapla
            var rawMoneyFlow = new List<decimal>();
            for (int i = 0; i < typicalPrices.Count; i++)
            {
                decimal moneyFlow = typicalPrices[i] * candles[i].Volume;
                rawMoneyFlow.Add(moneyFlow);
            }

            // Positive ve Negative Money Flow hesapla
            for (int i = _period; i < candles.Count; i++)
            {
                decimal positiveFlow = 0m;
                decimal negativeFlow = 0m;

                for (int j = i - _period + 1; j <= i; j++)
                {
                    if (j == 0) continue;

                    if (typicalPrices[j] > typicalPrices[j - 1])
                        positiveFlow += rawMoneyFlow[j];
                    else if (typicalPrices[j] < typicalPrices[j - 1])
                        negativeFlow += rawMoneyFlow[j];
                }

                decimal moneyRatio = negativeFlow == 0 ? 100m : positiveFlow / negativeFlow;
                decimal mfiValue = 100m - (100m / (1m + moneyRatio));

                var previousValue = results.LastOrDefault()?.Value ?? 0m;
                
                results.Add(new MFIResult(_overboughtLevel, _oversoldLevel)
                {
                    Time = candles[i].CloseTime,
                    Value = mfiValue,
                    PositiveFlow = positiveFlow,
                    NegativeFlow = negativeFlow,
                    MoneyRatio = moneyRatio,
                    Signal = DetermineSignal(mfiValue, previousValue)
                });
            }

            return results;
        }

        private SignalType DetermineSignal(decimal currentValue, decimal previousValue)
        {
            if (currentValue <= _oversoldLevel && previousValue > _oversoldLevel)
                return SignalType.Buy;
            if (currentValue >= _overboughtLevel && previousValue < _overboughtLevel)
                return SignalType.Sell;
            return SignalType.Neutral;
        }

        // Trend analizi
        public FlowAnalysis AnalyzeFlow(List<MFIResult> results, int lookback = 10)
        {
            if (results.Count < lookback) 
                return new FlowAnalysis { IsValid = false };

            var recentResults = results.TakeLast(lookback).ToList();

            return new FlowAnalysis
            {
                IsValid = true,
                MoneyFlowStrength = CalculateFlowStrength(recentResults),
                FlowBalance = CalculateFlowBalance(recentResults),
                IsAccumulating = IsAccumulating(recentResults),
                VolumeQuality = AssessVolumeQuality(recentResults)
            };
        }

        public class FlowAnalysis
        {
            public bool IsValid { get; set; }
            public decimal MoneyFlowStrength { get; set; }
            public decimal FlowBalance { get; set; }
            public bool IsAccumulating { get; set; }
            public decimal VolumeQuality { get; set; }
        }

        private decimal CalculateFlowStrength(List<MFIResult> results)
        {
            return results.Average(r => Math.Abs(r.Value - 50m));
        }

        private decimal CalculateFlowBalance(List<MFIResult> results)
        {
            decimal totalPositive = results.Sum(r => r.PositiveFlow);
            decimal totalNegative = results.Sum(r => r.NegativeFlow);
            return totalNegative == 0 ? 1m : totalPositive / totalNegative;
        }

        private bool IsAccumulating(List<MFIResult> results)
        {
            if (results.Count < 3) return false;
            var last3 = results.TakeLast(3).ToList();
            return last3[0].Value < last3[1].Value && last3[1].Value < last3[2].Value;
        }

        private decimal AssessVolumeQuality(List<MFIResult> results)
        {
            decimal highVolume = results.Max(r => r.PositiveFlow + r.NegativeFlow);
            return results.Average(r => (r.PositiveFlow + r.NegativeFlow) / highVolume);
        }
    }
}
