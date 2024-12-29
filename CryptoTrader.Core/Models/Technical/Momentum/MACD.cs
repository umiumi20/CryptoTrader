using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class MACD
    {
        private readonly int _fastPeriod;
        private readonly int _slowPeriod;
        private readonly int _signalPeriod;

        public MACD(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            _fastPeriod = fastPeriod;
            _slowPeriod = slowPeriod;
            _signalPeriod = signalPeriod;
        }

        public class MACDResult
        {
            public DateTime Time { get; set; }
            public decimal MACDLine { get; set; }
            public decimal SignalLine { get; set; }
            public decimal Histogram { get; set; }
            public bool IsPositive => MACDLine > SignalLine;
            public bool IsNegative => MACDLine < SignalLine;
            public SignalType Signal { get; set; }
        }

        public enum SignalType
        {
            Buy,
            Sell,
            Neutral
        }

        private List<decimal> CalculateEMA(List<decimal> prices, int period)
        {
            var ema = new List<decimal>();
            var multiplier = 2m / (period + 1);

            // lk EMA değeri SMA olarak hesaplanır
            var firstEMA = prices.Take(period).Average();
            ema.Add(firstEMA);

            // Kalan EMA değerlerini hesapla
            for (int i = period; i < prices.Count; i++)
            {
                var value = (prices[i] - ema[i - period]) * multiplier + ema[i - period];
                ema.Add(value);
            }

            return ema;
        }

        public List<MACDResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<MACDResult>();
            if (candles.Count <= _slowPeriod)
                return results;

            var prices = candles.Select(x => x.Close).ToList();
            
            // EMA hesaplamaları
            var fastEMA = CalculateEMA(prices, _fastPeriod);
            var slowEMA = CalculateEMA(prices, _slowPeriod);

            // MACD Line hesaplama
            var macdValues = new List<decimal>();
            for (int i = 0; i < fastEMA.Count; i++)
            {
                var macd = fastEMA[i] - slowEMA[i];
                macdValues.Add(macd);
            }

            // Signal Line hesaplama (MACD'nin EMA'sı)
            var signalLine = CalculateEMA(macdValues, _signalPeriod);

            // Sonuçları oluştur
            for (int i = 0; i < signalLine.Count; i++)
            {
                var macd = macdValues[i + macdValues.Count - signalLine.Count];
                var signal = signalLine[i];
                var histogram = macd - signal;

                var signalType = SignalType.Neutral;
                if (histogram > 0 && histogram > macdValues[i - 1] - signalLine[i - 1])
                    signalType = SignalType.Buy;
                else if (histogram < 0 && histogram < macdValues[i - 1] - signalLine[i - 1])
                    signalType = SignalType.Sell;

                results.Add(new MACDResult
                {
                    Time = candles[i + _slowPeriod].CloseTime,
                    MACDLine = macd,
                    SignalLine = signal,
                    Histogram = histogram,
                    Signal = signalType
                });
            }

            return results;
        }
    }
}
