using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class MassIndex
    {
        private readonly int _emaPeriod;
        private readonly int _summationPeriod;
        private readonly decimal _threshold;

        public MassIndex(int emaPeriod = 9, int summationPeriod = 25, decimal threshold = 27.0m)
        {
            _emaPeriod = emaPeriod;
            _summationPeriod = summationPeriod;
            _threshold = threshold;
        }

        public class MassIndexResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public string TrendState { get; set; } = string.Empty;
            public string TrendDirection { get; set; } = string.Empty;
        }

        public List<MassIndexResult> Calculate(List<BaseCandle> candles)
        {
            if (candles == null || candles.Count < _summationPeriod)
                return new List<MassIndexResult>();

            var results = new List<MassIndexResult>();
            var hlDiff = candles.Select(c => c.High - c.Low).ToList();
            
            var singleEma = CalculateEMA(hlDiff, _emaPeriod);
            var doubleEma = CalculateEMA(singleEma, _emaPeriod);
            
            var emaRatio = new List<decimal>();
            for (int i = 0; i < singleEma.Count && i < doubleEma.Count; i++)
            {
                if (doubleEma[i] != 0)
                    emaRatio.Add(singleEma[i] / doubleEma[i]);
                else
                    emaRatio.Add(0);
            }

            for (int i = _summationPeriod - 1; i < emaRatio.Count; i++)
            {
                var sum = emaRatio.Skip(i - _summationPeriod + 1).Take(_summationPeriod).Sum();
                
                var result = new MassIndexResult
                {
                    Time = candles[i].Time,
                    Value = sum,
                    TrendState = sum > _threshold ? "Reversal Zone" : "Normal Zone",
                    TrendDirection = DetermineTrendDirection(sum, i > 0 ? results.LastOrDefault()?.Value ?? 0 : 0)
                };

                results.Add(result);
            }

            return results;
        }

        private List<decimal> CalculateEMA(List<decimal> values, int period)
        {
            var results = new List<decimal>();
            var multiplier = 2.0m / (period + 1);
            
            // lk EMA değeri olarak SMA kullan
            var firstValue = values.Take(period).Average();
            results.Add(firstValue);

            for (int i = period; i < values.Count; i++)
            {
                var ema = (values[i] - results.Last()) * multiplier + results.Last();
                results.Add(ema);
            }

            return results;
        }

        private string DetermineTrendDirection(decimal currentValue, decimal previousValue)
        {
            if (currentValue > previousValue)
                return "Up";
            if (currentValue < previousValue)
                return "Down";
            return "Sideways";
        }
    }
}
