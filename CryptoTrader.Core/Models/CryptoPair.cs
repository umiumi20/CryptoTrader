namespace CryptoTrader.Core.Models
{
    public class CryptoPair
    {
        public string Symbol { get; set; } = string.Empty;
        public decimal LastPrice { get; set; }
        public decimal PriceChangePercent { get; set; }
        public decimal Volume { get; set; }
        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }
        public decimal BidPrice { get; set; }
        public decimal AskPrice { get; set; }
    }
}