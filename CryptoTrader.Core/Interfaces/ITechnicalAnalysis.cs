using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Interfaces
{
    public interface ITechnicalAnalysis
    {
        Task<Dictionary<string, decimal>> CalculateMomentumIndicators(List<BaseCandle> candles);
        Task<Dictionary<string, decimal>> CalculateVolumeIndicators(List<BaseCandle> candles);
        Task<Dictionary<string, decimal>> CalculateVolatilityIndicators(List<BaseCandle> candles);
        Task<Dictionary<string, List<decimal>>> CalculateOverlapIndicators(List<BaseCandle> candles);
        Task<Dictionary<string, decimal>> CalculatePerformanceMetrics(List<BaseCandle> candles);
    }
}
