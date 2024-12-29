using System;

namespace CryptoTrader.Core.Models.Technical.Momentum
{
    public class ElderRayIndex
    {
        private class ElderRayResult
        {
            public DateTime Time { get; set; }
            public decimal BullPower { get; set; }
            public decimal BearPower { get; set; }
            public string Analysis { get; set; } = string.Empty;
            public string DominantPower { get; set; } = string.Empty;
        }

        // ... diğer kodlar aynı kalacak
    }
}
