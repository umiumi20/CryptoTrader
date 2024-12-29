using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Services
{
    public class BinanceService : IBinanceService
    {
        private readonly Random _random = new Random();
        private readonly Dictionary<string, decimal> _lastPrices = new Dictionary<string, decimal>();
        private readonly Dictionary<string, decimal> _initialPrices = new Dictionary<string, decimal>
        {
            {"BTC/USDT", 43500m},
            {"ETH/USDT", 2250m},
            {"BNB/USDT", 275m},
            {"XRP/USDT", 0.65m},
            {"SOL/USDT", 110m},
            {"ADA/USDT", 0.65m},
            {"DOGE/USDT", 0.095m},
            {"DOT/USDT", 8.25m},
            {"MATIC/USDT", 0.85m},
            {"LINK/USDT", 15.75m}
        };

        public async Task<List<CryptoPair>> GetTopCryptoPairs()
        {
            var pairs = new List<CryptoPair>();
            
            foreach (var initialPrice in _initialPrices)
            {
                var symbol = initialPrice.Key;
                decimal currentPrice;

                if (_lastPrices.TryGetValue(symbol, out decimal lastPrice))
                {
                    // Rastgele fiyat değişimi (%0.1 ile %1 arasında)
                    var changePercent = (_random.NextDouble() * 1.9) - 0.95; // -0.95% ile +0.95% arası
                    var priceChange = lastPrice * (decimal)(changePercent / 100.0);
                    currentPrice = lastPrice + priceChange;
                }
                else
                {
                    // İlk fiyat
                    currentPrice = initialPrice.Value;
                }

                // 24 saatlik değişim yüzdesi
                var percentChange = _lastPrices.ContainsKey(symbol)
                    ? ((currentPrice - _lastPrices[symbol]) / _lastPrices[symbol]) * 100
                    : 0m;

                var pair = new CryptoPair
                {
                    Symbol = symbol,
                    LastPrice = currentPrice,
                    PriceChangePercent = percentChange,
                    Volume = currentPrice * _random.Next(1000, 5000),
                    HighPrice = _lastPrices.ContainsKey(symbol)
                        ? Math.Max(currentPrice, _lastPrices[symbol] * 1.02m)
                        : currentPrice * 1.02m,
                    LowPrice = _lastPrices.ContainsKey(symbol)
                        ? Math.Min(currentPrice, _lastPrices[symbol] * 0.98m)
                        : currentPrice * 0.98m,
                    BidPrice = currentPrice * 0.9995m,
                    AskPrice = currentPrice * 1.0005m
                };

                _lastPrices[symbol] = currentPrice;
                pairs.Add(pair);
            }

            await Task.Delay(100); // Simüle edilmiş ağ gecikmesi
            return pairs;
        }
    }
}