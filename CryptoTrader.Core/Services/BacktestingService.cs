using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Backtest;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Services
{
    public class BacktestingService
    {
        public class BacktestResults
        {
            public List<Strategy.Position> Positions { get; set; } = new();
            public decimal InitialBalance { get; set; }
            public decimal FinalBalance { get; set; }
            public decimal WinRate { get; set; }
            public decimal MaxDrawdown { get; set; }
            public decimal TotalReturn => (FinalBalance - InitialBalance) / InitialBalance * 100;
            public int TotalTrades => Positions.Count;
            public decimal AverageReturn => TotalTrades == 0 ? 0 : Positions.Average(p => p.ProfitLoss);
        }

        public BacktestResults RunBacktest(Strategy strategy, List<BaseCandle> candles)
        {
            strategy.Candles = candles;
            strategy.CalculateIndicators();

            for (int i = 0; i < candles.Count; i++)
            {
                strategy.Next(i);
            }

            return new BacktestResults
            {
                Positions = strategy.Positions,
                InitialBalance = strategy.InitialBalance,
                FinalBalance = strategy.CurrentBalance,
                WinRate = strategy.GetWinRate(),
                MaxDrawdown = strategy.MaxDrawdown
            };
        }
    }
}
