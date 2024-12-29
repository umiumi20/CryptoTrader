using System;

namespace CryptoTrader.Core.Models.Backtest
{
    public class TradePosition
    {
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal ExitPrice { get; set; }
        public decimal Quantity { get; set; }
        public PositionType Type { get; set; }
        public decimal PnL { get; set; }
        public decimal Commission { get; set; }
    }

    public enum PositionType
    {
        Long,
        Short
    }
}
