using System.Threading.Tasks;
using CryptoTrader.Models;

namespace CryptoTrader.Services
{
    public interface ITradingService
    {
        Task<decimal> GetCurrentPrice(string symbol);
        Task<bool> PlaceOrder(string symbol, decimal amount, string orderType);
    }

    public class TradingService : ITradingService
    {
        public async Task<decimal> GetCurrentPrice(string symbol)
        {
            // TODO: Implement real price fetching
            return 42000.00m;
        }

        public async Task<bool> PlaceOrder(string symbol, decimal amount, string orderType)
        {
            // TODO: Implement real order placement
            return true;
        }
    }
}
