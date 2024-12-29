using System;
using System.Collections.Generic;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Backtest
{
    public abstract class Strategy
    {
        public List<BaseCandle> Candles { get; set; } = new List<BaseCandle>();
        public List<Position> Positions { get; set; } = new List<Position>();
        public Dictionary<string, object> Indicators { get; set; } = new Dictionary<string, object>();
        
        public decimal InitialBalance { get; set; }
        public decimal CurrentBalance { get; private set; }
        public decimal MaxDrawdown { get; private set; }
        public int WinningTrades { get; private set; }
        public int LosingTrades { get; private set; }
        
        protected Strategy(decimal initialBalance = 10000m)
        {
            InitialBalance = initialBalance;
            CurrentBalance = initialBalance;
            Positions = new List<Position>();
            Candles = new List<BaseCandle>();
            Indicators = new Dictionary<string, object>();
            Init();
        }

        protected virtual void Init()
        {
            // Alt sınıflarda override edilebilir
        }

        public virtual void CalculateIndicators()
        {
            // Alt sınıflarda override edilebilir
        }

        public virtual void Next(int index)
        {
            // Alt sınıflarda override edilebilir
        }

        public void UpdateMetrics(Position position)
        {
            if (position.ProfitLoss > 0)
                WinningTrades++;
            else if (position.ProfitLoss < 0)
                LosingTrades++;

            CurrentBalance += position.ProfitLoss;
            
            decimal drawdown = (InitialBalance - CurrentBalance) / InitialBalance;
            MaxDrawdown = Math.Max(MaxDrawdown, drawdown);
        }

        public class Position
        {
            public DateTime EntryTime { get; set; }
            public DateTime? ExitTime { get; set; }
            public decimal EntryPrice { get; set; }
            public decimal? ExitPrice { get; set; }
            public decimal Quantity { get; set; }
            public bool IsLong { get; set; }
            public decimal ProfitLoss { get; set; }
            public string SignalSource { get; set; } = "Unknown";
        }

        public bool OpenPosition(DateTime time, decimal price, decimal quantity, bool isLong, string signalSource)
        {
            if (CurrentBalance <= 0) return false;

            var position = new Position
            {
                EntryTime = time,
                EntryPrice = price,
                Quantity = quantity,
                IsLong = isLong,
                SignalSource = signalSource
            };

            Positions.Add(position);
            return true;
        }

        public bool ClosePosition(DateTime time, decimal price, Position position)
        {
            if (position.ExitTime.HasValue) return false;

            position.ExitTime = time;
            position.ExitPrice = price;

            decimal profitLoss;
            if (position.IsLong)
                profitLoss = (price - position.EntryPrice) * position.Quantity;
            else
                profitLoss = (position.EntryPrice - price) * position.Quantity;

            position.ProfitLoss = profitLoss;
            UpdateMetrics(position);

            return true;
        }

        public decimal GetWinRate()
        {
            int totalTrades = WinningTrades + LosingTrades;
            return totalTrades == 0 ? 0 : (decimal)WinningTrades / totalTrades;
        }

        public Position? GetLastPosition()
        {
            return Positions.Count > 0 ? Positions[^1] : null;
        }

        public bool HasOpenPosition()
        {
            var lastPosition = GetLastPosition();
            return lastPosition != null && !lastPosition.ExitTime.HasValue;
        }
    }
}
