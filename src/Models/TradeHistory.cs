namespace CryptoTrader.Models
{
    public class TradeHistory
    {
        public string Id { get; set; }
        public string BotId { get; set; }
        public string Symbol { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public string TradeType { get; set; }
        public string TradeTime { get; set; }
    }
}
