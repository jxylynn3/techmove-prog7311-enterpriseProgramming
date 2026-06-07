using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ST10448420_TechMove_GLMS.ApiServices
{
    // Replaces all direct _context.ServiceRequests queries in the MVC project.
    // Calls the backend Web API over HTTP. No database access here.
    public class ApiServiceRequestService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiServiceRequestService(
            IHttpClientFactory factory,
            IHttpContextAccessor accessor)
        {
            _httpClientFactory = factory;
            _httpContextAccessor = accessor;
        }

        private HttpClient CreateAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("GlmsApi");
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<List<ServiceRequestApiDTO>> GetAllAsync()
        {// Note: In a real app, we would not fetch ALL service requests and filter in memory.
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync("api/servicerequests");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ServiceRequestApiDTO>>(json, JsonOpts)
                       ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiServiceRequestService] GetAll failed: {ex.Message}");
                return new();
            }
        }

        public async Task<List<ServiceRequestApiDTO>> GetByContractAsync(int contractId)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync(
                    $"api/servicerequests/bycontract/{contractId}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ServiceRequestApiDTO>>(json, JsonOpts)
                       ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[ApiServiceRequestService] GetByContract failed: {ex.Message}");
                return new();
            }
        }

        public async Task<(ServiceRequestApiDTO? dto, string? error)> CreateAsync(
            CreateServiceRequestApiDTO _dto)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var content = new StringContent(
                    JsonSerializer.Serialize(_dto), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/servicerequests", content);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    return (null, $"API error: {body}");
                }

                var json = await response.Content.ReadAsStringAsync();
                return (JsonSerializer.Deserialize<ServiceRequestApiDTO>(json, JsonOpts), null);
            }
            catch (Exception ex)
            {
                return (null, $"Network error: {ex.Message}");
            }
        }

        public async Task<(bool success, string? error)> UpdateStatusAsync(int id, string status)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var payload = new StringContent(
                    JsonSerializer.Serialize(new { status }),
                    Encoding.UTF8, "application/json");
                var response = await client.PatchAsync(
                    $"api/servicerequests/{id}/status", payload);

                if (!response.IsSuccessStatusCode)
                    return (false, $"API returned {(int)response.StatusCode}");

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Network error: {ex.Message}");
            }
        }
    }


}