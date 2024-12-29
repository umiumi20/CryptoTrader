using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class BRAR
    {
        private readonly int _period;
        private readonly decimal _highThreshold;
        private readonly decimal _lowThreshold;

        public BRAR(int period = 26, decimal highThreshold = 300m, decimal lowThreshold = 50m)
        {
            _period = period;
            _highThreshold = highThreshold;
            _lowThreshold = lowThreshold;
        }

        public class BRARResult
        {
            public DateTime Time { get; set; }
            public decimal BR { get; set; } // Buying Ratio
            public decimal AR { get; set; } // Advance Ratio
            public decimal BuyingPower { get; set; }
            public decimal SellingPower { get; set; }
            public SignalType Signal { get; set; }
            public bool IsOverbought => BR > _highThreshold;
            public bool IsOversold => BR < _lowThreshold;
            private readonly decimal _highThreshold;
            private readonly decimal _lowThreshold;

            public BRARResult(decimal high, decimal low)
            {
                _highThreshold = high;
                _lowThreshold = low;
            }
        }

        public enum SignalType { Buy, Sell, Neutral }

        public List<BRARResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<BRARResult>();
            if (candles.Count < _period + 1) return results;

            for (int i = _period; i < candles.Count; i++)
            {
                decimal sumBuyingPower = 0;
                decimal sumSellingPower = 0;
                decimal sumAdvance = 0;
                decimal sumDecline = 0;

                for (int j = 0; j < _period; j++)
                {
                    var currentCandle = candles[i - j];
                    var previousClose = candles[i - j - 1].Close;

                    // BR calculation components
                    decimal buyingPower = Math.Max(0, currentCandle.High - previousClose);
                    decimal sellingPower = Math.Max(0, previousClose - currentCandle.Low);
                    sumBuyingPower += buyingPower;
                    sumSellingPower += sellingPower;

                    // AR calculation components
                    decimal advance = Math.Max(0, currentCandle.High - currentCandle.Open);
                    decimal decline = Math.Max(0, currentCandle.Open - currentCandle.Low);
                    sumAdvance += advance;
                    sumDecline += decline;
                }

                decimal br = sumSellingPower == 0 ? 100 : (sumBuyingPower / sumSellingPower) * 100;
                decimal ar = sumDecline == 0 ? 100 : (sumAdvance / sumDecline) * 100;

                results.Add(new BRARResult(_highThreshold, _lowThreshold)
                {
                    Time = candles[i].CloseTime,
                    BR = br,
                    AR = ar,
                    BuyingPower = sumBuyingPower,
                    SellingPower = sumSellingPower,
                    Signal = DetermineSignal(br, ar)
                });
            }

            return results;
        }

        private SignalType DetermineSignal(decimal br, decimal ar)
        {
            if (br < _lowThreshold && ar < _lowThreshold) return SignalType.Buy;
            if (br > _highThreshold && ar > _highThreshold) return SignalType.Sell;
            return SignalType.Neutral;
        }
    }
}
