using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GLMS.Tests.IntegrationTests
{
    public abstract class IntegrationTestBase : IDisposable
    {
        protected readonly HttpClient _client;
        protected string _token = string.Empty;
        private readonly JsonSerializerOptions _jsonOptions;

        protected IntegrationTestBase()
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://localhost:5266/"); // Make sure this matches your API port
            _client.Timeout = TimeSpan.FromSeconds(30);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        protected async Task AuthenticateAsync()
        {
            var loginData = new { username = "admin", password = "password" };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("api/Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponse>(json, _jsonOptions);
                _token = result?.token ?? string.Empty;
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        protected StringContent GetJsonContent(object obj)
        {
            return new StringContent(JsonSerializer.Serialize(obj, _jsonOptions), Encoding.UTF8, "application/json");
        }

        protected async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }

        private class LoginResponse
        {
            public string token { get; set; } = string.Empty;
            public string username { get; set; } = string.Empty;
        }
    }
}