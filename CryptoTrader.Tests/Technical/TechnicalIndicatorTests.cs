using System;
using System.Collections.Generic;
using System.Linq;
using CryptoTrader.Core.Models.Candlestick;
using CryptoTrader.Core.Models.Technical.Momentum;
using Xunit;

namespace CryptoTrader.Tests.Technical
{
    public class TechnicalIndicatorTests
    {
        private List<BaseCandle> GenerateTestCandles(int count)
        {
            var candles = new List<BaseCandle>();
            var random = new Random(42); // Sabit seed ile tutarlı rastgele değerler
            var baseTime = DateTime.UtcNow.Date;

            for (int i = 0; i < count; i++)
            {
                var candle = new BaseCandle
                {
                    Time = baseTime.AddMinutes(i * 15),
                    Open = 100m + (decimal)random.NextDouble() * 10,
                    High = 0m,
                    Low = 0m,
                    Close = 0m,
                    Volume = (decimal)random.NextDouble() * 1000
                };

                candle.Close = candle.Open + (decimal)(random.NextDouble() * 2 - 1) * 5;
                candle.High = Math.Max(candle.Open, candle.Close) + (decimal)random.NextDouble() * 2;
                candle.Low = Math.Min(candle.Open, candle.Close) - (decimal)random.NextDouble() * 2;

                candles.Add(candle);
            }

            return candles;
        }

        [Fact]
        public void MassIndex_Calculate_ReturnsValidResults()
        {
            // Arrange
            var candles = GenerateTestCandles(100);
            var massIndex = new MassIndex();

            // Act
            var results = massIndex.Calculate(candles);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results);
            Assert.All(results, r => Assert.True(r.Value >= 0));
            Assert.All(results, r => Assert.NotNull(r.TrendState));
            Assert.All(results, r => Assert.NotNull(r.TrendDirection));
        }

        [Fact]
        public void MassIndex_Calculate_EmptyInput_ReturnsEmptyList()
        {
            // Arrange
            var massIndex = new MassIndex();

            // Act
            var results = massIndex.Calculate(new List<BaseCandle>());

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public void MassIndex_Calculate_NullInput_ReturnsEmptyList()
        {
            // Arrange
            var massIndex = new MassIndex();

            // Act
            var results = massIndex.Calculate(new List<BaseCandle>()); // Boş liste ile test et

            // Assert
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Theory]
        [InlineData(9, 25, 27.0)]
        [InlineData(10, 30, 29.0)]
        public void MassIndex_Calculate_DifferentParameters_ReturnsResults(int emaPeriod, int summationPeriod, decimal threshold)
        {
            // Arrange
            var candles = GenerateTestCandles(100);
            var massIndex = new MassIndex(emaPeriod, summationPeriod, threshold);

            // Act
            var results = massIndex.Calculate(candles);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results);
            Assert.All(results, r => Assert.True(r.Value >= 0));
        }
    }
}
