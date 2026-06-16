using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GLMS.Shared.Models;
using GLMS.Shared.ViewModels;

namespace GLMS.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string _token = string.Empty;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;

            // Use environment variable for Docker, fallback to localhost for development
            var apiUrl = Environment.GetEnvironmentVariable("ApiBaseAddress") ?? "http://localhost:8080";
            _httpClient.BaseAddress = new Uri(apiUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            Console.WriteLine($"API Base Address: {apiUrl}");
        }

        private async Task SetAuthHeader()
        {
            if (string.IsNullOrEmpty(_token))
            {
                _token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken") ?? string.Empty;
            }
            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);
            }
        }

        // ========== AUTHENTICATION ==========
        public async Task<string> LoginAsync(string username, string password)
        {
            var loginData = new { username, password };
            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                _token = result?.token ?? string.Empty;
                _httpContextAccessor.HttpContext?.Session.SetString("JWToken", _token);
                return _token;
            }
            return string.Empty;
        }

        // ========== CONTRACT METHODS ==========
        public async Task<List<Contract>> GetContractsAsync(DateTime? startDate = null, DateTime? endDate = null, ContractStatus? status = null)
        {
            await SetAuthHeader();
            var url = "api/Contracts";
            var queryParams = new List<string>();

            if (startDate.HasValue) queryParams.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue) queryParams.Add($"endDate={endDate.Value:yyyy-MM-dd}");
            if (status.HasValue) queryParams.Add($"status={status.Value}");

            if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Deserialize<List<Contract>>(json, options) ?? new List<Contract>();
        }

        public async Task<Contract> GetContractByIdAsync(int id)
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/Contracts/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Deserialize<Contract>(json, options) ?? new Contract();
        }

        public async Task<Contract> CreateContractAsync(Contract contract)
        {
            await SetAuthHeader();

            // Create DTO that matches API expected format
            var createRequest = new
            {
                clientId = contract.ClientId,
                contractNumber = contract.ContractNumber,
                startDate = contract.StartDate.ToString("yyyy-MM-dd"),
                endDate = contract.EndDate.ToString("yyyy-MM-dd"),
                status = contract.Status.ToString(),
                serviceLevel = contract.ServiceLevel,
                termsAndConditions = contract.TermsAndConditions,
                pdfFilePath = contract.PdfFilePath
            };

            var content = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Contracts", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Deserialize<Contract>(json, options) ?? contract;
        }

        // NEW: Update existing contract
        public async Task<bool> UpdateContractAsync(Contract contract)
        {
            await SetAuthHeader();

            var updateRequest = new
            {
                id = contract.Id,
                clientId = contract.ClientId,
                contractNumber = contract.ContractNumber,
                startDate = contract.StartDate.ToString("yyyy-MM-dd"),
                endDate = contract.EndDate.ToString("yyyy-MM-dd"),
                status = contract.Status.ToString(),
                serviceLevel = contract.ServiceLevel,
                termsAndConditions = contract.TermsAndConditions
            };

            var content = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/Contracts/{contract.Id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateContractStatusAsync(int id, ContractStatus status)
        {
            await SetAuthHeader();
            var updateData = new { status = status.ToString() };
            var content = new StringContent(JsonSerializer.Serialize(updateData), Encoding.UTF8, "application/json");
            var response = await _httpClient.PatchAsync($"api/Contracts/{id}/status", content);
            return response.IsSuccessStatusCode;
        }

        // NEW: Download PDF for contract
        public async Task<byte[]?> DownloadPdfAsync(int contractId)
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/Contracts/{contractId}/pdf");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            return null;
        }

        // ========== SERVICE REQUEST METHODS ==========
        public async Task<List<ServiceRequest>> GetServiceRequestsAsync()
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync("api/ServiceRequests");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Deserialize<List<ServiceRequest>>(json, options) ?? new List<ServiceRequest>();
        }

        // NEW: Get single service request by ID
        public async Task<ServiceRequest> GetServiceRequestByIdAsync(int id)
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/ServiceRequests/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Deserialize<ServiceRequest>(json, options) ?? new ServiceRequest();
        }

        public async Task<ServiceRequest> CreateServiceRequestAsync(ServiceRequest request)
        {
            await SetAuthHeader();

            var createRequest = new
            {
                contractId = request.ContractId,
                description = request.Description,
                amountUSD = request.AmountUSD,
                notes = request.Notes
            };

            var content = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/ServiceRequests", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            return JsonSerializer.Deserialize<ServiceRequest>(json, options) ?? request;
        }

        // ========== CURRENCY METHODS ==========
        public async Task<decimal> ConvertCurrencyAsync(decimal usdAmount)
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/ServiceRequests/currency/convert?usdAmount={usdAmount}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CurrencyResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result?.zarAmount ?? 0;
        }

        // ========== CLIENT METHODS ==========
        public async Task<List<Client>> GetClientsAsync()
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync("api/Clients");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<Client>>(json, options) ?? new List<Client>();
        }

        // NEW: Get single client by ID
        public async Task<Client> GetClientByIdAsync(int id)
        {
            await SetAuthHeader();
            var response = await _httpClient.GetAsync($"api/Clients/{id}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<Client>(json, options) ?? new Client();
        }

        public async Task<Client> CreateClientAsync(Client client)
        {
            await SetAuthHeader();
            var content = new StringContent(JsonSerializer.Serialize(client), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/Clients", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<Client>(json, options) ?? client;
        }

        // ========== HEALTH CHECK ==========
        public async Task<bool> IsApiHealthy()
        {
            try
            {
                var response = await _httpClient.GetAsync("health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ========== PRIVATE CLASSES FOR JSON DESERIALIZATION ==========
        private class LoginResponse
        {
            public string token { get; set; } = string.Empty;
        }

        private class CurrencyResponse
        {
            public decimal zarAmount { get; set; }
        }
    }
}