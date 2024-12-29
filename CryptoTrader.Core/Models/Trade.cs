using System;

namespace CryptoTrader.Core.Models
{
    public class Trade
    {
        // Temel trade özellikleri
        public string Symbol { get; set; }             // İşlem yapılacak coin çifti (örn: BTCUSDT)
        public decimal Price { get; set; }             // İşlem fiyatı
        public decimal Amount { get; set; }            // İşlem miktarı
        public decimal TotalValue => Price * Amount;    // Toplam işlem değeri
        
        // Trade detayları
        public TradeType Type { get; set; }           // Long veya Short pozisyon
        public DateTime Timestamp { get; set; }        // İşlem zamanı
        public int Leverage { get; set; }             // Kaldıraç oranı
        public bool IsCrossMargin { get; set; }       // Cross margin mi isolated margin mi?
        
        // Trade durumu
        public TradeStatus Status { get; set; }       // İşlem durumu
        public string OrderId { get; set; }           // Borsa tarafından verilen işlem ID'si
        
        // Risk yönetimi
        public decimal? StopLoss { get; set; }        // Zarar kes seviyesi
        public decimal? TakeProfit { get; set; }      // Kar al seviyesi

        // İşlem sonucu
        public decimal? PnL { get; set; }             // Kar/Zarar miktarı
        public decimal? PnLPercentage { get; set; }   // Kar/Zarar yüzdesi
    }

    public enum TradeType
    {
        Long,   // Yükseliş beklentisi
        Short   // Düşüş beklentisi
    }

    public enum TradeStatus
    {
        Pending,    // Beklemede
        Active,     // Aktif
        Completed,  // Tamamlandı
        Cancelled,  // İptal edildi
        Failed      // Başarısız
    }
}