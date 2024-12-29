using System;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class Inertia
    {
        public class InertiaResult
        {
            public DateTime Time { get; set; }
            public decimal Momentum { get; set; }
            public string MomentumPhase { get; set; } = string.Empty;
            public string TrendDirection { get; set; } = string.Empty;
        }

        // ... diğer kodlar aynı kalacak
    }
}
