using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace GLMS.Web.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CurrencyService> _logger;
        private decimal _cachedRate;
        private DateTime _lastFetch;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        public CurrencyService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<CurrencyService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            // Return cached rate if still valid
            if (_cachedRate > 0 && DateTime.UtcNow - _lastFetch < _cacheDuration)
            {
                return _cachedRate;
            }

            try
            {
                string apiUrl = _configuration["ExchangeApi:BaseUrl"];
                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Parse JSON response (ExchangeRate-API format)
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;

                if (root.TryGetProperty("rates", out JsonElement rates))
                {
                    if (rates.TryGetProperty("ZAR", out JsonElement zarRate))
                    {
                        _cachedRate = zarRate.GetDecimal();
                        _lastFetch = DateTime.UtcNow;
                        _logger.LogInformation($"Fetched USD/ZAR rate: {_cachedRate}");
                        return _cachedRate;
                    }
                }

                throw new Exception("ZAR rate not found in API response");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch exchange rate");

                // Fallback rate if API fails
                if (_cachedRate == 0)
                {
                    _cachedRate = 19.50m; // Fallback rate
                }
                return _cachedRate;
            }
        }

        public async Task<decimal> ConvertUsdToZarAsync(decimal usdAmount)
        {
            decimal rate = await GetUsdToZarRateAsync();
            return usdAmount * rate;
        }
    }
}