using System;
using System.Collections.Generic;

namespace CryptoTrader.Core.Models.Candlestick
{
    public class HeikinAshi : BaseCandle
    {
        public HeikinAshi() { }

        public HeikinAshi(BaseCandle previousCandle, BaseCandle currentCandle)
        {
            if (currentCandle == null)
                throw new ArgumentNullException(nameof(currentCandle));

            Time = currentCandle.Time;
            CloseTime = currentCandle.CloseTime;
            Open = previousCandle != null ? (previousCandle.Open + previousCandle.Close) / 2m : currentCandle.Open;
            Close = (currentCandle.Open + currentCandle.High + currentCandle.Low + currentCandle.Close) / 4m;
            High = Math.Max(Math.Max(currentCandle.High, Open), Close);
            Low = Math.Min(Math.Min(currentCandle.Low, Open), Close);
            Volume = currentCandle.Volume;
        }

        public static List<HeikinAshi> ConvertCandles(List<BaseCandle> candles)
        {
            if (candles == null || candles.Count == 0)
                return new List<HeikinAshi>();

            var result = new List<HeikinAshi>();
            HeikinAshi? previous = null;

            foreach (var candle in candles)
            {
                var heikinAshi = new HeikinAshi();
                
                if (previous == null)
                {
                    heikinAshi.Time = candle.Time;
                    heikinAshi.CloseTime = candle.CloseTime;
                    heikinAshi.Open = candle.Open;
                    heikinAshi.Close = (candle.Open + candle.High + candle.Low + candle.Close) / 4m;
                    heikinAshi.High = candle.High;
                    heikinAshi.Low = candle.Low;
                    heikinAshi.Volume = candle.Volume;
                }
                else
                {
                    heikinAshi.Time = candle.Time;
                    heikinAshi.CloseTime = candle.CloseTime;
                    heikinAshi.Open = (previous.Open + previous.Close) / 2m;
                    heikinAshi.Close = (candle.Open + candle.High + candle.Low + candle.Close) / 4m;
                    heikinAshi.High = Math.Max(candle.High, Math.Max(heikinAshi.Open, heikinAshi.Close));
                    heikinAshi.Low = Math.Min(candle.Low, Math.Min(heikinAshi.Open, heikinAshi.Close));
                    heikinAshi.Volume = candle.Volume;
                }

                result.Add(heikinAshi);
                previous = heikinAshi;
            }

            return result;
        }
    }
}
