using System;

namespace CryptoTrader.Core.Models.Candlestick
{
    public class CandlePattern
    {
        public string Name { get; set; } = string.Empty;
        public bool IsBullish { get; set; }
        public decimal Significance { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime Time { get; set; }
    }
}
