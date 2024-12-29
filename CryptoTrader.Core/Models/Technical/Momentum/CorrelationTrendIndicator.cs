using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class CorrelationTrendIndicator
    {
        private readonly int _period;
        private readonly decimal _threshold;

        public CorrelationTrendIndicator(int period = 20, decimal threshold = 0.3m)
        {
            _period = period;
            _threshold = threshold;
        }

        public class CTIResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal Correlation { get; set; }
            public decimal TrendStrength { get; set; }
            public SignalType Signal { get; set; }
            public bool IsStrongTrend => Math.Abs(Value) > _threshold;
            public TrendType CurrentTrend { get; set; }
            private readonly decimal _threshold;

            public CTIResult(decimal threshold)
            {
                _threshold = threshold;
            }
        }

        public enum SignalType { Buy, Sell, Neutral }
        public enum TrendType { Up, Down, Sideways }

        public List<CTIResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<CTIResult>();
            if (candles.Count < _period) return results;

            // Fiyat serisi oluştur
            var prices = candles.Select(c => c.Close).ToList();

            // Her period için hesaplama yap
            for (int i = _period - 1; i < prices.Count; i++)
            {
                var periodPrices = prices.Skip(i - _period + 1).Take(_period).ToList();
                var timeValues = Enumerable.Range(1, _period).Select(x => (decimal)x).ToList();

                // Korelasyon hesapla
                var correlation = CalculateCorrelation(timeValues, periodPrices);
                
                // Trend gücünü hesapla
                var trendStrength = CalculateTrendStrength(periodPrices);

                // CTI değerini hesapla
                var cti = correlation * trendStrength;

                var previousValue = results.LastOrDefault()?.Value ?? 0;

                results.Add(new CTIResult(_threshold)
                {
                    Time = candles[i].CloseTime,
                    Value = cti,
                    Correlation = correlation,
                    TrendStrength = trendStrength,
                    Signal = DetermineSignal(cti, previousValue),
                    CurrentTrend = DetermineTrend(cti)
                });
            }

            return results;
        }

        private decimal CalculateCorrelation(List<decimal> x, List<decimal> y)
        {
            if (x.Count != y.Count) return 0;

            int n = x.Count;
            decimal sumX = x.Sum();
            decimal sumY = y.Sum();
            decimal sumXY = x.Zip(y, (a, b) => a * b).Sum();
            decimal sumXX = x.Sum(a => a * a);
            decimal sumYY = y.Sum(b => b * b);

            decimal numerator = (n * sumXY) - (sumX * sumY);
            decimal denominator = (decimal)Math.Sqrt((double)((n * sumXX) - (sumX * sumX)) * 
                                                   (double)((n * sumYY) - (sumY * sumY)));

            if (denominator == 0) return 0;
            return numerator / denominator;
        }

        private decimal CalculateTrendStrength(List<decimal> prices)
        {
            if (prices.Count < 2) return 0;

            decimal firstPrice = prices.First();
            decimal lastPrice = prices.Last();
            decimal maxPrice = prices.Max();
            decimal minPrice = prices.Min();

            decimal priceRange = maxPrice - minPrice;
            if (priceRange == 0) return 0;

            decimal movement = Math.Abs(lastPrice - firstPrice);
            return movement / priceRange;
        }

        private SignalType DetermineSignal(decimal currentValue, decimal previousValue)
        {
            // Trend dönüş sinyalleri
            if (currentValue > _threshold && previousValue <= _threshold)
                return SignalType.Buy;
            if (currentValue < -_threshold && previousValue >= -_threshold)
                return SignalType.Sell;

            // Momentum sinyalleri
            if (currentValue > previousValue && currentValue > 0)
                return SignalType.Buy;
            if (currentValue < previousValue && currentValue < 0)
                return SignalType.Sell;

            return SignalType.Neutral;
        }

        private TrendType DetermineTrend(decimal cti)
        {
            if (cti > _threshold) return TrendType.Up;
            if (cti < -_threshold) return TrendType.Down;
            return TrendType.Sideways;
        }

        // Trend analizi metodları
        public bool IsStrongTrend(List<CTIResult> results, int lookback = 5)
        {
            if (results.Count < lookback) return false;
            var recentResults = results.TakeLast(lookback).ToList();
            return recentResults.All(r => Math.Abs(r.Value) > _threshold);
        }

        public decimal GetAverageTrendStrength(List<CTIResult> results, int lookback = 5)
        {
            if (results.Count < lookback) return 0;
            var recentResults = results.TakeLast(lookback).ToList();
            return recentResults.Average(r => Math.Abs(r.TrendStrength));
        }

        // Divergence analizi
        public bool IsBullishDivergence(List<CTIResult> ctiData, List<BaseCandle> priceData, int lookback = 5)
        {
            if (ctiData.Count < lookback || priceData.Count < lookback)
                return false;

            var recentCTI = ctiData.TakeLast(lookback).ToList();
            var recentPrices = priceData.TakeLast(lookback).ToList();

            decimal ctiLow1 = recentCTI.First().Value;
            decimal ctiLow2 = recentCTI.Last().Value;
            decimal priceLow1 = recentPrices.First().Low;
            decimal priceLow2 = recentPrices.Last().Low;

            return ctiLow2 > ctiLow1 && priceLow2 < priceLow1;
        }

        public bool IsBearishDivergence(List<CTIResult> ctiData, List<BaseCandle> priceData, int lookback = 5)
        {
            if (ctiData.Count < lookback || priceData.Count < lookback)
                return false;

            var recentCTI = ctiData.TakeLast(lookback).ToList();
            var recentPrices = priceData.TakeLast(lookback).ToList();

            decimal ctiHigh1 = recentCTI.First().Value;
            decimal ctiHigh2 = recentCTI.Last().Value;
            decimal priceHigh1 = recentPrices.First().High;
            decimal priceHigh2 = recentPrices.Last().High;

            return ctiHigh2 < ctiHigh1 && priceHigh2 > priceHigh1;
        }

        // Volatilite analizi
        public decimal CalculateVolatility(List<CTIResult> results, int lookback = 5)
        {
            if (results.Count < lookback) return 0;
            var recentResults = results.TakeLast(lookback).ToList();
            var values = recentResults.Select(r => r.Value).ToList();
            var mean = values.Average();
            return (decimal)Math.Sqrt((double)values.Sum(v => (v - mean) * (v - mean)) / lookback);
        }
    }
}
