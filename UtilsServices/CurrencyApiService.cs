using System.Text.Json;

namespace ST10448420_TechMove_GLMS.UtilsServices
{
    public class CurrencyApiService
    {
        private readonly HttpClient _httpClient;

        public CurrencyApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetRateAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(
                    "https://api.exchangerate-api.com/v4/latest/USD");

                var data = JsonDocument.Parse(response);

                var rate = data.RootElement
                    .GetProperty("rates")
                    .GetProperty("ZAR")
                    .GetDecimal();

                return rate;
            }
            catch
            {
                // fallback if API dies
                return 18.5m;
            }
        }
    }
}