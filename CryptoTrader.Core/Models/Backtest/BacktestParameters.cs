using System;
using System.Collections.Generic;

namespace CryptoTrader.Core.Models.Backtest
{
    public class BacktestParameters
    {
        public decimal InitialCash { get; set; }
        public decimal Commission { get; set; }
        public bool ExclusiveOrders { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
