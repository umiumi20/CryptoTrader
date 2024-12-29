using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class RSI
    {
        private readonly int _period;
        private readonly int _overboughtLevel;
        private readonly int _oversoldLevel;

        public RSI(int period = 14, int overboughtLevel = 70, int oversoldLevel = 30)
        {
            _period = period;
            _overboughtLevel = overboughtLevel;
            _oversoldLevel = oversoldLevel;
        }

        public class RSIResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public bool IsOverbought => Value >= _overboughtLevel;
            public bool IsOversold => Value <= _oversoldLevel;
            public decimal UpwardForce { get; set; }
            public decimal DownwardForce { get; set; }
            public SignalType Signal { get; set; }
            private readonly int _overboughtLevel;
            private readonly int _oversoldLevel;

            public RSIResult(int overbought, int oversold)
            {
                _overboughtLevel = overbought;
                _oversoldLevel = oversold;
            }
        }

        public enum SignalType
        {
            Buy,
            Sell,
            Neutral
        }

        public List<RSIResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<RSIResult>();
            if (candles.Count <= _period)
                return results;

            var gains = new List<decimal>();
            var losses = new List<decimal>();

            // lk değişimleri hesapla
            for (int i = 1; i < candles.Count; i++)
            {
                var change = candles[i].Close - candles[i - 1].Close;
                gains.Add(Math.Max(change, 0));
                losses.Add(Math.Max(-change, 0));
            }

            // lk ortalama kazanç ve kayıpları hesapla
            var avgGain = gains.Take(_period).Average();
            var avgLoss = losses.Take(_period).Average();

            // lk RSI değerini hesapla
            var rsi = CalculateRSI(avgGain, avgLoss);
            results.Add(new RSIResult(_overboughtLevel, _oversoldLevel)
            {
                Time = candles[_period].CloseTime,
                Value = rsi,
                UpwardForce = avgGain,
                DownwardForce = avgLoss,
                Signal = DetermineSignal(rsi)
            });

            // Kalan değerleri hesapla
            for (int i = _period + 1; i < candles.Count; i++)
            {
                avgGain = ((avgGain * (_period - 1)) + gains[i - 1]) / _period;
                avgLoss = ((avgLoss * (_period - 1)) + losses[i - 1]) / _period;

                rsi = CalculateRSI(avgGain, avgLoss);
                results.Add(new RSIResult(_overboughtLevel, _oversoldLevel)
                {
                    Time = candles[i].CloseTime,
                    Value = rsi,
                    UpwardForce = avgGain,
                    DownwardForce = avgLoss,
                    Signal = DetermineSignal(rsi)
                });
            }

            return results;
        }

        private decimal CalculateRSI(decimal avgGain, decimal avgLoss)
        {
            if (avgLoss == 0) return 100;
            var rs = avgGain / avgLoss;
            return 100 - (100 / (1 + rs));
        }

        private SignalType DetermineSignal(decimal rsi)
        {
            if (rsi <= _oversoldLevel) return SignalType.Buy;
            if (rsi >= _overboughtLevel) return SignalType.Sell;
            return SignalType.Neutral;
        }
    }
}
