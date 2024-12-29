using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Technical.Trend;

namespace CryptoTrader.Core.Models.Backtest
{
    public class SmaCrossStrategy : Strategy
    {
        private readonly int _fastPeriod;
        private readonly int _slowPeriod;
        private List<decimal> _fastSma;
        private List<decimal> _slowSma;

        public SmaCrossStrategy(int fastPeriod = 10, int slowPeriod = 20, decimal initialBalance = 10000m)
            : base(initialBalance)
        {
            _fastPeriod = fastPeriod;
            _slowPeriod = slowPeriod;
            _fastSma = new List<decimal>();
            _slowSma = new List<decimal>();
        }

        public override void CalculateIndicators()
        {
            if (Candles.Count < _slowPeriod) return;

            var prices = Candles.Select(c => c.Close).ToList();
            
            // Fast SMA hesapla
            _fastSma = CalculateSMA(prices, _fastPeriod);
            
            // Slow SMA hesapla
            _slowSma = CalculateSMA(prices, _slowPeriod);

            Indicators["FastSMA"] = _fastSma;
            Indicators["SlowSMA"] = _slowSma;
        }

        public override void Next(int index)
        {
            if (index < _slowPeriod) return;

            var currentFastSma = _fastSma[index - _slowPeriod + _fastPeriod];
            var currentSlowSma = _slowSma[index - _slowPeriod];
            var previousFastSma = _fastSma[index - _slowPeriod + _fastPeriod - 1];
            var previousSlowSma = _slowSma[index - _slowPeriod - 1];

            // Altın Çapraz (Golden Cross)
            if (currentFastSma > currentSlowSma && previousFastSma <= previousSlowSma)
            {
                if (!HasOpenPosition())
                {
                    OpenPosition(
                        Candles[index].CloseTime,
                        Candles[index].Close,
                        CalculatePositionSize(),
                        true,
                        "GoldenCross"
                    );
                }
            }
            // Ölüm Çaprazı (Death Cross)
            else if (currentFastSma < currentSlowSma && previousFastSma >= previousSlowSma)
            {
                var position = GetLastPosition();
                if (position != null && !position.ExitTime.HasValue)
                {
                    ClosePosition(Candles[index].CloseTime, Candles[index].Close, position);
                }
            }
        }

        private List<decimal> CalculateSMA(List<decimal> prices, int period)
        {
            var sma = new List<decimal>();
            if (prices.Count < period) return sma;

            decimal sum = prices.Take(period).Sum();
            sma.Add(sum / period);

            for (int i = period; i < prices.Count; i++)
            {
                sum = sum - prices[i - period] + prices[i];
                sma.Add(sum / period);
            }

            return sma;
        }

        private decimal CalculatePositionSize()
        {
            // Basit pozisyon boyutu hesaplama - bakiyenin %2'si
            return CurrentBalance * 0.02m;
        }
    }
}
