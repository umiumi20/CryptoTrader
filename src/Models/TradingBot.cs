namespace CryptoTrader.Models
{
    public class TradingBot
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Strategy { get; set; }
        public bool IsActive { get; set; }
        public decimal CurrentBalance { get; set; }
        public string LastTradeTime { get; set; }
    }
}
