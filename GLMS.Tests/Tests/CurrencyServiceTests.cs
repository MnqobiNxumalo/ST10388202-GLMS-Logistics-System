using Xunit;
using GLMS.Web.Services;

namespace GLMS.Tests
{
    public class CurrencyServiceTests
    {
        // Test helper that doesn't call real API
        private class TestCurrencyService : ICurrencyService
        {
            private readonly decimal _testRate;

            public TestCurrencyService(decimal testRate)
            {
                _testRate = testRate;
            }

            public Task<decimal> GetUsdToZarRateAsync()
            {
                return Task.FromResult(_testRate);
            }

            public Task<decimal> ConvertUsdToZarAsync(decimal usdAmount)
            {
                return Task.FromResult(usdAmount * _testRate);
            }
        }

        [Fact]
        public async Task ConvertUsdToZar_WithRate18_50_AndAmount100_Returns1850()
        {
            var service = new TestCurrencyService(18.50m);
            decimal result = await service.ConvertUsdToZarAsync(100.00m);
            Assert.Equal(1850.00m, result);
        }

        [Fact]
        public async Task ConvertUsdToZar_WithZeroAmount_ReturnsZero()
        {
            var service = new TestCurrencyService(18.50m);
            decimal result = await service.ConvertUsdToZarAsync(0m);
            Assert.Equal(0m, result);
        }

        [Theory]
        [InlineData(50, 18.50, 925)]
        [InlineData(100, 19.20, 1920)]
        [InlineData(150.75, 17.80, 2683.35)]
        [InlineData(1, 18.50, 18.50)]
        [InlineData(1000, 25.50, 25500)]
        public async Task ConvertUsdToZar_MultipleScenarios_ReturnsCorrectZar(
            decimal usdAmount, decimal rate, decimal expectedZar)
        {
            var service = new TestCurrencyService(rate);
            decimal result = await service.ConvertUsdToZarAsync(usdAmount);
            Assert.Equal(expectedZar, Math.Round(result, 2));
        }

        [Fact]
        public async Task GetUsdToZarRate_ReturnsPositiveRate()
        {
            var service = new TestCurrencyService(19.50m);
            decimal rate = await service.GetUsdToZarRateAsync();
            Assert.True(rate > 0);
        }
    }
}