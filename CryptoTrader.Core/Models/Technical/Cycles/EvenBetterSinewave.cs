using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;

namespace CryptoTrader.Core.Models.Technical.Cycles
{
    public class EvenBetterSinewave
    {
        private readonly int _period;
        private readonly int _signalPeriod;

        public EvenBetterSinewave(int period = 20, int signalPeriod = 3)
        {
            _period = period;
            _signalPeriod = signalPeriod;
        }

        public class SinewaveResult
        {
            public DateTime Time { get; set; }
            public decimal Sine { get; set; }
            public decimal Lead { get; set; }
            public SignalType Signal { get; set; }
            public CyclePhase Phase { get; set; }
            public bool IsCrossover { get; set; }
            public decimal PreviousSine { get; set; }
            public decimal PreviousLead { get; set; }
        }

        public enum SignalType
        {
            Buy,
            Sell,
            Hold
        }

        public enum CyclePhase
        {
            Rising,
            Falling,
            Neutral
        }

        public List<SinewaveResult> Calculate(List<BaseCandle> candles)
        {
            var results = new List<SinewaveResult>();
            if (candles.Count < _period) return results;

            var prices = candles.Select(c => c.Close).ToList();
            var hp = CalculateHP(prices);
            var filtered = new List<decimal>();

            // Filtreleme
            for (int i = 0; i < hp.Count; i++)
            {
                decimal filtered_value = (decimal)(0.075 * (double)hp[i] * (double)hp[i]);
                filtered.Add(filtered_value);
            }

            // Sinewave hesaplama
            var sine = new List<decimal>();
            var lead = new List<decimal>();

            for (int i = _period; i < filtered.Count; i++)
            {
                decimal currentSine = (decimal)Math.Sin((double)filtered[i]);
                decimal currentLead = (decimal)Math.Sin((double)filtered[i] + Math.PI / 4);

                sine.Add(currentSine);
                lead.Add(currentLead);
            }

            // Sonuçları oluştur
            decimal previousSine = 0;
            decimal previousLead = 0;

            for (int i = 0; i < sine.Count; i++)
            {
                var currentIndex = i + _period;
                var isCrossover = (sine[i] > lead[i] && previousSine <= previousLead) ||
                                (sine[i] < lead[i] && previousSine >= previousLead);

                results.Add(new SinewaveResult
                {
                    Time = candles[currentIndex].CloseTime,
                    Sine = sine[i],
                    Lead = lead[i],
                    Signal = DetermineSignal(sine[i], lead[i], previousSine, previousLead),
                    Phase = DeterminePhase(sine[i], lead[i]),
                    IsCrossover = isCrossover,
                    PreviousSine = previousSine,
                    PreviousLead = previousLead
                });

                previousSine = sine[i];
                previousLead = lead[i];
            }

            return results;
        }

        private List<decimal> CalculateHP(List<decimal> prices)
        {
            var hp = new List<decimal>();
            var alpha1 = (decimal)(0.0962);
            var alpha2 = (decimal)(0.5769);
            decimal a2 = 0, a3 = 0, a4 = 0;
            decimal b2 = 0, b3 = 0, b4 = 0;

            for (int i = 0; i < prices.Count; i++)
            {
                decimal current = prices[i];
                decimal temp = current;

                a2 = (2 * (1 - alpha1)) * temp + ((alpha1 * alpha1) - 2 * alpha1) * a2 + (1 - alpha1) * (1 - alpha1) * a3;
                a3 = (2 * (1 - alpha1)) * a2 + ((alpha1 * alpha1) - 2 * alpha1) * a3 + (1 - alpha1) * (1 - alpha1) * a4;
                a4 = (2 * (1 - alpha1)) * a3 + ((alpha1 * alpha1) - 2 * alpha1) * a4;

                b2 = (2 * (1 - alpha2)) * temp + ((alpha2 * alpha2) - 2 * alpha2) * b2 + (1 - alpha2) * (1 - alpha2) * b3;
                b3 = (2 * (1 - alpha2)) * b2 + ((alpha2 * alpha2) - 2 * alpha2) * b3 + (1 - alpha2) * (1 - alpha2) * b4;
                b4 = (2 * (1 - alpha2)) * b3 + ((alpha2 * alpha2) - 2 * alpha2) * b4;

                decimal hpValue = a4 - b4;
                hp.Add(hpValue);
            }

            return hp;
        }

        private SignalType DetermineSignal(decimal sine, decimal lead, decimal previousSine, decimal previousLead)
        {
            if (sine > lead && previousSine <= previousLead)
                return SignalType.Buy;
            if (sine < lead && previousSine >= previousLead)
                return SignalType.Sell;
            return SignalType.Hold;
        }

        private CyclePhase DeterminePhase(decimal sine, decimal lead)
        {
            if (sine > 0 && lead > 0)
                return CyclePhase.Rising;
            if (sine < 0 && lead < 0)
                return CyclePhase.Falling;
            return CyclePhase.Neutral;
        }
    }
}
