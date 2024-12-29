using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class CenterOfGravity
    {
        private readonly int _period;

        public CenterOfGravity(int period = 10)
        {
            _period = period;
        }

        public class CGResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal WeightedSum { get; set; }
            public decimal PriceSum { get; set; }
            public SignalType Signal { get; set; }
            public bool IsBullish => Value > 0;
            public bool IsBearish => Value < 0;
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<CGResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<CGResult>();
            if (candles.Count < _period) return results;

            for (int i = _period - 1; i < candles.Count; i++)
            {
                decimal weightedSum = 0;
                decimal priceSum = 0;

                // Son _period kadar mumu al
                var periodCandles = candles.Skip(i - _period + 1).Take(_period).ToList();

                for (int j = 0; j < _period; j++)
                {
                    var price = periodCandles[j].Close;
                    var weight = _period - j;
                    
                    weightedSum += price * weight;
                    priceSum += price;
                }

                // CG değerini hesapla
                decimal cg = -weightedSum / priceSum;
                var previousCG = results.LastOrDefault()?.Value ?? 0;

                results.Add(new CGResult
                {
                    Time = candles[i].CloseTime,
                    Value = cg,
                    WeightedSum = weightedSum,
                    PriceSum = priceSum,
                    Signal = DetermineSignal(cg, previousCG)
                });
            }

            return results;
        }

        private SignalType DetermineSignal(decimal currentCG, decimal previousCG)
        {
            // Zero-line cross sinyalleri
            if (currentCG > 0 && previousCG <= 0) return SignalType.Buy;
            if (currentCG < 0 && previousCG >= 0) return SignalType.Sell;

            // Momentum sinyalleri
            if (currentCG > previousCG && currentCG > 0) return SignalType.Buy;
            if (currentCG < previousCG && currentCG < 0) return SignalType.Sell;

            return SignalType.Neutral;
        }

        public List<CGResult> CalculateSmoothed(List<BaseCandle> candles, int smoothPeriod = 3)
        {
            var rawCG = Calculate(candles);
            var smoothed = new List<CGResult>();

            for (int i = smoothPeriod - 1; i < rawCG.Count; i++)
            {
                var periodValues = rawCG.Skip(i - smoothPeriod + 1).Take(smoothPeriod);
                var avgCG = periodValues.Average(x => x.Value);

                var smoothedResult = new CGResult
                {
                    Time = rawCG[i].Time,
                    Value = avgCG,
                    WeightedSum = rawCG[i].WeightedSum,
                    PriceSum = rawCG[i].PriceSum,
                    Signal = DetermineSignal(avgCG, smoothed.LastOrDefault()?.Value ?? 0)
                };

                smoothed.Add(smoothedResult);
            }

            return smoothed;
        }
    }
}
