using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class ChandeMomentumOscillator
    {
        private readonly int _period;
        private readonly decimal _overboughtLevel;
        private readonly decimal _oversoldLevel;

        public ChandeMomentumOscillator(int period = 9, decimal overboughtLevel = 50m, decimal oversoldLevel = -50m)
        {
            _period = period;
            _overboughtLevel = overboughtLevel;
            _oversoldLevel = oversoldLevel;
        }

        public class CMOResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal UpSum { get; set; }
            public decimal DownSum { get; set; }
            public decimal AbsoluteValue => Math.Abs(Value);
            public SignalType Signal { get; set; }
            public bool IsOverbought => Value >= _overboughtLevel;
            public bool IsOversold => Value <= _oversoldLevel;
            public bool IsDiverging => Math.Abs(Value) > 50;
            private readonly decimal _overboughtLevel;
            private readonly decimal _oversoldLevel;

            public CMOResult(decimal overbought, decimal oversold)
            {
                _overboughtLevel = overbought;
                _oversoldLevel = oversold;
            }
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<CMOResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<CMOResult>();
            if (candles.Count < _period + 1) return results;

            // Fiyat değişimlerini hesapla
            var priceChanges = new List<decimal>();
            for (int i = 1; i < candles.Count; i++)
            {
                priceChanges.Add(candles[i].Close - candles[i - 1].Close);
            }

            // Her period için CMO hesapla
            for (int i = _period; i < priceChanges.Count; i++)
            {
                var periodChanges = priceChanges.Skip(i - _period).Take(_period);
                
                decimal upSum = periodChanges.Where(x => x > 0).Sum();
                decimal downSum = Math.Abs(periodChanges.Where(x => x < 0).Sum());
                
                // CMO değerini hesapla: ((UpSum - DownSum) / (UpSum + DownSum)) * 100
                decimal cmo = 0;
                if (upSum + downSum != 0)
                {
                    cmo = ((upSum - downSum) / (upSum + downSum)) * 100;
                }

                var previousValue = results.LastOrDefault()?.Value ?? 0;

                results.Add(new CMOResult(_overboughtLevel, _oversoldLevel)
                {
                    Time = candles[i + 1].CloseTime,
                    Value = cmo,
                    UpSum = upSum,
                    DownSum = downSum,
                    Signal = DetermineSignal(cmo, previousValue)
                });
            }

            return results;
        }

        private SignalType DetermineSignal(decimal currentValue, decimal previousValue)
        {
            // Aşırı alım/satım sinyalleri
            if (currentValue <= _oversoldLevel) return SignalType.Buy;
            if (currentValue >= _overboughtLevel) return SignalType.Sell;

            // Momentum dönüş sinyalleri
            if (previousValue <= _oversoldLevel && currentValue > _oversoldLevel)
                return SignalType.Buy;
            if (previousValue >= _overboughtLevel && currentValue < _overboughtLevel)
                return SignalType.Sell;

            // Sıfır çizgisi geçişleri
            if (previousValue < 0 && currentValue > 0) return SignalType.Buy;
            if (previousValue > 0 && currentValue < 0) return SignalType.Sell;

            return SignalType.Neutral;
        }

        // Divergence analizi
        public bool CheckBullishDivergence(List<CMOResult> cmoData, List<BaseCandle> priceData, int lookbackPeriod = 14)
        {
            if (cmoData.Count < lookbackPeriod || priceData.Count < lookbackPeriod)
                return false;

            var recentCMO = cmoData.TakeLast(lookbackPeriod).ToList();
            var recentPrice = priceData.TakeLast(lookbackPeriod).ToList();

            decimal cmoLow1 = recentCMO[0].Value;
            decimal cmoLow2 = recentCMO[lookbackPeriod - 1].Value;
            decimal priceLow1 = recentPrice[0].Low;
            decimal priceLow2 = recentPrice[lookbackPeriod - 1].Low;

            // Pozitif uyumsuzluk: CMO yükseliyor, fiyat düşüyor
            return cmoLow2 > cmoLow1 && priceLow2 < priceLow1;
        }

        public bool CheckBearishDivergence(List<CMOResult> cmoData, List<BaseCandle> priceData, int lookbackPeriod = 14)
        {
            if (cmoData.Count < lookbackPeriod || priceData.Count < lookbackPeriod)
                return false;

            var recentCMO = cmoData.TakeLast(lookbackPeriod).ToList();
            var recentPrice = priceData.TakeLast(lookbackPeriod).ToList();

            decimal cmoHigh1 = recentCMO[0].Value;
            decimal cmoHigh2 = recentCMO[lookbackPeriod - 1].Value;
            decimal priceHigh1 = recentPrice[0].High;
            decimal priceHigh2 = recentPrice[lookbackPeriod - 1].High;

            // Negatif uyumsuzluk: CMO düşüyor, fiyat yükseliyor
            return cmoHigh2 < cmoHigh1 && priceHigh2 > priceHigh1;
        }

        // Trend analizi
        public bool IsBullishTrend(List<CMOResult> results, int lookback = 5)
        {
            if (results.Count < lookback) return false;
            var recentResults = results.TakeLast(lookback).ToList();
            return recentResults.All(r => r.Value > 0) && 
                   recentResults.Last().Value > recentResults.First().Value;
        }

        public bool IsBearishTrend(List<CMOResult> results, int lookback = 5)
        {
            if (results.Count < lookback) return false;
            var recentResults = results.TakeLast(lookback).ToList();
            return recentResults.All(r => r.Value < 0) && 
                   recentResults.Last().Value < recentResults.First().Value;
        }

        // Momentum gücü analizi
        public decimal CalculateMomentumStrength(List<CMOResult> results, int lookback = 5)
        {
            if (results.Count < lookback) return 0;
            var recentResults = results.TakeLast(lookback).ToList();
            return recentResults.Average(r => r.AbsoluteValue);
        }

        // Volatilite analizi
        public decimal CalculateVolatility(List<CMOResult> results, int lookback = 5)
        {
            if (results.Count < lookback) return 0;
            var recentResults = results.TakeLast(lookback).ToList();
            var values = recentResults.Select(r => r.Value).ToList();
            var mean = values.Average();
            return (decimal)Math.Sqrt((double)values.Sum(v => (v - mean) * (v - mean)) / lookback);
        }
    }
}
