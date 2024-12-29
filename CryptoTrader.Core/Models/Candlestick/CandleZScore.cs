using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoTrader.Core.Models.Candlestick
{
    public class CandleZScore
    {
        public DateTime Time { get; set; }
        public decimal Value { get; set; }
        public decimal ZScore { get; set; }

        public static List<CandleZScore> Calculate(List<BaseCandle> candles, int period = 20)
        {
            var zScores = new List<CandleZScore>();
            
            for (int i = period - 1; i < candles.Count; i++)
            {
                var periodCandles = candles.Skip(i - period + 1).Take(period);
                var values = periodCandles.Select(c => c.Close);
                
                decimal mean = values.Average();
                decimal stdDev = (decimal)Math.Sqrt((double)values.Select(x => Math.Pow((double)(x - mean), 2)).Average());
                
                if (stdDev == 0) stdDev = 1; // Sıfıra bölünmeyi önle
                
                zScores.Add(new CandleZScore
                {
                    Time = candles[i].CloseTime,
                    Value = candles[i].Close,
                    ZScore = (candles[i].Close - mean) / stdDev
                });
            }
            
            return zScores;
        }
    }
}
