using System;
using System.Collections.Generic;

namespace CryptoTrader.Core.Models.Backtest
{
    public class BacktestResult
    {
        public BacktestResult()
        {
            MonthlyPnL = new Dictionary<DateTime, decimal>();
            Positions = new List<TradePosition>();
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPnL { get; set; }
        public decimal MaxDrawdown { get; set; }
        public decimal SharpeRatio { get; set; }
        public decimal WinRate { get; set; }
        public int TotalTradingDays { get; set; }
        public int WinningDays { get; set; }
        public int LosingDays { get; set; }
        public decimal MonthlyProfit { get; set; }
        public decimal MonthlyROI { get; set; }
        public decimal MaxDailyProfit { get; set; }
        public decimal MaxDailyLoss { get; set; }
        public Dictionary<DateTime, decimal> MonthlyPnL { get; set; }
        public List<TradePosition> Positions { get; set; }
    }
}
