using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class CoppockCurve
    {
        private readonly int _longRoCPeriod;
        private readonly int _shortRoCPeriod;
        private readonly int _wmaPeriod;

        public CoppockCurve(int longRoCPeriod = 14, int shortRoCPeriod = 11, int wmaPeriod = 10)
        {
            _longRoCPeriod = longRoCPeriod;
            _shortRoCPeriod = shortRoCPeriod;
            _wmaPeriod = wmaPeriod;
        }

        public class CoppockResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal LongRoC { get; set; }
            public decimal ShortRoC { get; set; }
            public SignalType Signal { get; set; }
            public bool IsBullish => Value > 0;
            public bool IsBearish => Value < 0;
            public decimal Momentum => LongRoC + ShortRoC;
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<CoppockResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<CoppockResult>();
            if (candles.Count < Math.Max(_longRoCPeriod, _shortRoCPeriod) + _wmaPeriod)
                return results;

            // Fiyat listesini al
            var prices = candles.Select(c => c.Close).ToList();

            // RoC değerlerini hesapla
            var longRoC = CalculateRoC(prices, _longRoCPeriod);
            var shortRoC = CalculateRoC(prices, _shortRoCPeriod);

            // RoC'ları topla
            var rocSum = new List<decimal>();
            int startIndex = Math.Max(_longRoCPeriod, _shortRoCPeriod) - 1;
            for (int i = 0; i < longRoC.Count; i++)
            {
                rocSum.Add(longRoC[i] + shortRoC[i]);
            }

            // WMA hesapla
            var wma = CalculateWMA(rocSum, _wmaPeriod);

            // Sonuçları oluştur
            for (int i = 0; i < wma.Count; i++)
            {
                var currentIndex = startIndex + _wmaPeriod + i;
                var previousValue = results.LastOrDefault()?.Value ?? 0;

                results.Add(new CoppockResult
                {
                    Time = candles[currentIndex].CloseTime,
                    Value = wma[i],
                    LongRoC = longRoC[i + _wmaPeriod - 1],
                    ShortRoC = shortRoC[i + _wmaPeriod - 1],
                    Signal = DetermineSignal(wma[i], previousValue)
                });
            }

            return results;
        }

        private List<decimal> CalculateRoC(List<decimal> prices, int period)
        {
            var roc = new List<decimal>();
            
            for (int i = period; i < prices.Count; i++)
            {
                decimal currentPrice = prices[i];
                decimal previousPrice = prices[i - period];
                decimal value = ((currentPrice - previousPrice) / previousPrice) * 100;
                roc.Add(value);
            }

            return roc;
        }

        private List<decimal> CalculateWMA(List<decimal> values, int period)
        {
            var wma = new List<decimal>();
            if (values.Count < period) return wma;

            decimal weightSum = (period * (period + 1)) / 2m;

            for (int i = period - 1; i < values.Count; i++)
            {
                decimal sum = 0;
                for (int j = 0; j < period; j++)
                {
                    sum += values[i - j] * (period - j);
                }
                wma.Add(sum / weightSum);
            }

            return wma;
        }

        private SignalType DetermineSignal(decimal currentValue, decimal previousValue)
        {
            // Sıfır çizgisi geçişleri
            if (previousValue <= 0 && currentValue > 0)
                return SignalType.Buy;
            if (previousValue >= 0 && currentValue < 0)
                return SignalType.Sell;

            // Momentum değişimleri
            if (currentValue > previousValue && currentValue > 0)
                return SignalType.Buy;
            if (currentValue < previousValue && currentValue < 0)
                return SignalType.Sell;

            return SignalType.Neutral;
        }

        // Trend analizi
        public bool IsBullishTrend(List<CoppockResult> results, int lookback = 3)
        {
            if (results.Count < lookback) return false;
            var recentResults = results.TakeLast(lookback).ToList();
            return recentResults.All(r => r.Value > 0) && 
                   recentResults.Last().Value > recentResults.First().Value;
        }

        public bool IsBearishTrend(List<CoppockResult> results, int lookback = 3)
        {
            if (results.Count < lookback) return false;
            var recentResults = results.TakeLast(lookback).ToList();
            return recentResults.All(r => r.Value < 0) && 
                   recentResults.Last().Value < recentResults.First().Value;
        }

        // Güç analizi
        public decimal CalculateTrendStrength(List<CoppockResult> results, int lookback = 5)
        {
            if (results.Count < lookback) return 0;
            var recentResults = results.TakeLast(lookback).ToList();
            
            decimal firstValue = recentResults.First().Value;
            decimal lastValue = recentResults.Last().Value;
            
            return ((lastValue - firstValue) / Math.Abs(firstValue)) * 100;
        }

        // Divergence analizi
        public bool IsBullishDivergence(List<CoppockResult> coppockData, List<BaseCandle> priceData, int lookback = 5)
        {
            if (coppockData.Count < lookback || priceData.Count < lookback) 
                return false;

            var recentCoppock = coppockData.TakeLast(lookback).ToList();
            var recentPrice = priceData.TakeLast(lookback).ToList();

            decimal coppockLow1 = recentCoppock.First().Value;
            decimal coppockLow2 = recentCoppock.Last().Value;
            decimal priceLow1 = recentPrice.First().Low;
            decimal priceLow2 = recentPrice.Last().Low;

            return coppockLow2 > coppockLow1 && priceLow2 < priceLow1;
        }

        public bool IsBearishDivergence(List<CoppockResult> coppockData, List<BaseCandle> priceData, int lookback = 5)
        {
            if (coppockData.Count < lookback || priceData.Count < lookback) 
                return false;

            var recentCoppock = coppockData.TakeLast(lookback).ToList();
            var recentPrice = priceData.TakeLast(lookback).ToList();

            decimal coppockHigh1 = recentCoppock.First().Value;
            decimal coppockHigh2 = recentCoppock.Last().Value;
            decimal priceHigh1 = recentPrice.First().High;
            decimal priceHigh2 = recentPrice.Last().High;

            return coppockHigh2 < coppockHigh1 && priceHigh2 > priceHigh1;
        }
    }
}
