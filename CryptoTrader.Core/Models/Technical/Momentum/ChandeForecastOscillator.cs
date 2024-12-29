using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class ChandeForecastOscillator
    {
        private readonly int _period;

        public ChandeForecastOscillator(int period = 14)
        {
            _period = period;
        }

        public class CFOResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal Forecast { get; set; }
            public decimal Slope { get; set; }
            public decimal Intercept { get; set; }
            public SignalType Signal { get; set; }
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<CFOResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<CFOResult>();
            if (candles.Count < _period) return results;

            for (int i = _period - 1; i < candles.Count; i++)
            {
                var periodPrices = candles.Skip(i - _period + 1).Take(_period).Select(x => x.Close).ToList();
                var (slope, intercept) = CalculateLinearRegression(periodPrices);
                
                var forecast = slope * (_period + 1) + intercept;
                var lastPrice = periodPrices.Last();
                var cfo = ((forecast - lastPrice) / lastPrice) * 100;

                results.Add(new CFOResult
                {
                    Time = candles[i].CloseTime,
                    Value = cfo,
                    Forecast = forecast,
                    Slope = slope,
                    Intercept = intercept,
                    Signal = DetermineSignal(cfo)
                });
            }

            return results;
        }

        private (decimal slope, decimal intercept) CalculateLinearRegression(List<decimal> prices)
        {
            int n = prices.Count;
            decimal sumX = 0;
            decimal sumY = 0;
            decimal sumXY = 0;
            decimal sumXX = 0;

            for (int i = 0; i < n; i++)
            {
                decimal x = i + 1;
                decimal y = prices[i];

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumXX += x * x;
            }

            decimal slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
            decimal intercept = (sumY - slope * sumX) / n;

            return (slope, intercept);
        }

        private SignalType DetermineSignal(decimal cfo)
        {
            if (cfo > 0.5m) return SignalType.Buy;
            if (cfo < -0.5m) return SignalType.Sell;
            return SignalType.Neutral;
        }
    }
}
