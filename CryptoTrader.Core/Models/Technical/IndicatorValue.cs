using System;

namespace CryptoTrader.Core.Models.Technical
{
    public class IndicatorValue
    {
        public DateTime Time { get; set; }
        public decimal Value { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
    }
}
