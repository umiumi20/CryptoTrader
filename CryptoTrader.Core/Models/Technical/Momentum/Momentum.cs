using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class Momentum
    {
        private readonly int _period;

        public Momentum(int period = 10)
        {
            _period = period;
        }

        public class MomentumResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public decimal Normalized { get; set; }
            public SignalType Signal { get; set; }
        }

        public enum SignalType
        {
            Buy,
            Sell,
            Neutral
        }

        public List<MomentumResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<MomentumResult>();
            if (candles.Count <= _period)
                return results;

            for (int i = _period; i < candles.Count; i++)
            {
                decimal momentum = candles[i].Close - candles[i - _period].Close;
                decimal normalized = (candles[i].Close / candles[i - _period].Close) * 100;

                var signal = SignalType.Neutral;
                if (momentum > 0 && normalized > 100) signal = SignalType.Buy;
                else if (momentum < 0 && normalized < 100) signal = SignalType.Sell;

                results.Add(new MomentumResult
                {
                    Time = candles[i].CloseTime,
                    Value = momentum,
                    Normalized = normalized,
                    Signal = signal
                });
            }

            return results;
        }
    }
}
