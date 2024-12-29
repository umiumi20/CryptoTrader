using System;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class EfficiencyRatio
    {
        public class EfficiencyRatioResult
        {
            public DateTime Time { get; set; }
            public decimal Value { get; set; }
            public string VolatilityTrend { get; set; } = string.Empty;
        }

        // ... diğer kodlar aynı kalacak
    }
}
