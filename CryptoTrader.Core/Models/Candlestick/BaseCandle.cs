using System;

namespace CryptoTrader.Core.Models.Candlestick
{
    public class BaseCandle
    {
        public DateTime Time { get; set; }
        public DateTime CloseTime { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }

        public bool IsBullish => Close > Open;
        public bool IsBearish => Close < Open;
        public decimal BodySize => Math.Abs(Close - Open);
        public decimal UpperShadow => High - Math.Max(Open, Close);
        public decimal LowerShadow => Math.Min(Open, Close) - Low;
        public decimal TotalRange => High - Low;
    }
}
