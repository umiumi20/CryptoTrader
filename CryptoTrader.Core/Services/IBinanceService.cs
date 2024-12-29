using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoTrader.Core.Models;

namespace CryptoTrader.Core.Services
{
    public interface IBinanceService
    {
        Task<List<CryptoPair>> GetTopCryptoPairs();
    }
}