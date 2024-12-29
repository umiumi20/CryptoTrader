using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class AbsolutePriceOscillator
    {
        private readonly int _fastPeriod;
        private readonly int _slowPeriod;

        public AbsolutePriceOscillator(int fastPeriod = 10, int slowPeriod = 21)
        {
            _fastPeriod = fastPeriod;
            _slowPeriod = slowPeriod;
        }

        public class APOResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public SignalType Signal { get; set; }
            public bool IsBullish => Value > 0;
            public bool IsBearish => Value < 0;
            public decimal FastEMA { get; set; }
            public decimal SlowEMA { get; set; }
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<APOResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<APOResult>();
            if (candles.Count < _slowPeriod) return results;

            var prices = candles.Select(x => x.Close).ToList();
            var fastEMA = CalculateEMA(prices, _fastPeriod);
            var slowEMA = CalculateEMA(prices, _slowPeriod);

            for (int i = _slowPeriod - 1; i < prices.Count; i++)
            {
                var apo = fastEMA[i - (_slowPeriod - _fastPeriod)] - slowEMA[i];
                var previousAPO = results.LastOrDefault()?.Value ?? 0;

                var result = new APOResult
                {
                    Time = candles[i].CloseTime,
                    Value = apo,
                    Signal = DetermineSignal(apo, previousAPO),
                    FastEMA = fastEMA[i - (_slowPeriod - _fastPeriod)],
                    SlowEMA = slowEMA[i]
                };

                results.Add(result);
            }

            return results;
        }

        private List<decimal> CalculateEMA(List<decimal> prices, int period)
        {
            var results = new List<decimal>();
            var multiplier = 2.0m / (period + 1);

            // lk EMA değeri SMA olarak hesaplanır
            var firstEMA = prices.Take(period).Average();
            results.Add(firstEMA);

            for (int i = period; i < prices.Count; i++)
            {
                var ema = (prices[i] - results.Last()) * multiplier + results.Last();
                results.Add(ema);
            }

            return results;
        }

        private SignalType DetermineSignal(decimal currentAPO, decimal previousAPO)
        {
            if (currentAPO > previousAPO && currentAPO > 0) return SignalType.Buy;
            if (currentAPO < previousAPO && currentAPO < 0) return SignalType.Sell;
            return SignalType.Neutral;
        }
    }
}
