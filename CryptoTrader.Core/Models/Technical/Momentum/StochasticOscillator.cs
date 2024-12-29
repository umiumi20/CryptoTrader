using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class StochasticOscillator
    {
        private readonly int _kPeriod;
        private readonly int _dPeriod;
        private readonly int _smooth;

        public StochasticOscillator(int kPeriod = 14, int dPeriod = 3, int smooth = 3)
        {
            _kPeriod = kPeriod;
            _dPeriod = dPeriod;
            _smooth = smooth;
        }

        public class StochResult
        {
            public DateTime Time { get; set; }
            public decimal K { get; set; }  // Fast Stochastic
            public decimal D { get; set; }  // Slow Stochastic
            public bool IsOverbought => K >= 80;
            public bool IsOversold => K <= 20;
            public SignalType Signal { get; set; }
        }

        public enum SignalType
        {
            Buy,
            Sell,
            Neutral
        }

        public List<StochResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<StochResult>();
            if (candles.Count <= _kPeriod)
                return results;

            // %K hesaplama
            var rawK = new List<decimal>();
            for (int i = _kPeriod - 1; i < candles.Count; i++)
            {
                var high = candles.Skip(i - _kPeriod + 1).Take(_kPeriod).Max(x => x.High);
                var low = candles.Skip(i - _kPeriod + 1).Take(_kPeriod).Min(x => x.Low);
                var close = candles[i].Close;

                decimal k = high - low != 0 ? 
                    ((close - low) / (high - low)) * 100 : 
                    0;
                
                rawK.Add(k);
            }

            // %K Smoothing
            var smoothK = new List<decimal>();
            for (int i = _smooth - 1; i < rawK.Count; i++)
            {
                var avg = rawK.Skip(i - _smooth + 1).Take(_smooth).Average();
                smoothK.Add(avg);
            }

            // %D hesaplama (smoothK'nın SMA'sı)
            for (int i = _dPeriod - 1; i < smoothK.Count; i++)
            {
                var d = smoothK.Skip(i - _dPeriod + 1).Take(_dPeriod).Average();
                var k = smoothK[i];

                var signal = SignalType.Neutral;
                if (k < 20 && k > d) signal = SignalType.Buy;
                else if (k > 80 && k < d) signal = SignalType.Sell;

                results.Add(new StochResult
                {
                    Time = candles[i + _kPeriod + _smooth - 1].CloseTime,
                    K = k,
                    D = d,
                    Signal = signal
                });
            }

            return results;
        }
    }
}
