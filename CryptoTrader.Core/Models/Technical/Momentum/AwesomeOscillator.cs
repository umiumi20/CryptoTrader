using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class AwesomeOscillator
    {
        private readonly int _fastPeriod;
        private readonly int _slowPeriod;

        public AwesomeOscillator(int fastPeriod = 5, int slowPeriod = 34)
        {
            _fastPeriod = fastPeriod;
            _slowPeriod = slowPeriod;
        }

        public class AOResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public bool IsBullish { get; set; }
            public bool IsBearish { get; set; }
            public SignalType Signal { get; set; }
            public decimal Momentum { get; set; }
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<AOResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<AOResult>();
            if (candles.Count < _slowPeriod) return results;

            // Medyan fiyatları hesapla ((High + Low) / 2)
            var medianPrices = candles.Select(c => (c.High + c.Low) / 2).ToList();

            // SMA hesaplamaları
            var fastSMA = CalculateSMA(medianPrices, _fastPeriod);
            var slowSMA = CalculateSMA(medianPrices, _slowPeriod);

            // AO değerlerini hesapla
            for (int i = _slowPeriod - 1; i < medianPrices.Count; i++)
            {
                var ao = fastSMA[i - _slowPeriod + _fastPeriod] - slowSMA[i];
                var previousAO = i > _slowPeriod - 1 ? results.LastOrDefault()?.Value ?? 0 : 0;

                var result = new AOResult
                {
                    Time = candles[i].CloseTime,
                    Value = ao,
                    IsBullish = ao > previousAO,
                    IsBearish = ao < previousAO,
                    Signal = DetermineSignal(ao, previousAO),
                    Momentum = ao - previousAO
                };

                results.Add(result);
            }

            return results;
        }

        private List<decimal> CalculateSMA(List<decimal> values, int period)
        {
            var results = new List<decimal>();
            for (int i = period - 1; i < values.Count; i++)
            {
                var sum = values.Skip(i - period + 1).Take(period).Sum();
                results.Add(sum / period);
            }
            return results;
        }

        private SignalType DetermineSignal(decimal currentAO, decimal previousAO)
        {
            if (currentAO > previousAO && currentAO > 0) return SignalType.Buy;
            if (currentAO < previousAO && currentAO < 0) return SignalType.Sell;
            return SignalType.Neutral;
        }
    }
}
