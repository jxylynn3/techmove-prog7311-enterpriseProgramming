using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
namespace ST10448420_TechMove_GLMS.APIServices
{
// Replaces all direct _context.ServiceRequests queries in the MVC project.
    public class ApiServiceRequestService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiServiceRequestService(IHttpClientFactory factory, IHttpContextAccessor accessor)
        {
            _httpClientFactory = factory;
            _httpContextAccessor = accessor;
        }

        private HttpClient CreateAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("GlmsApi");
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<List<ServiceRequestApiDTO>> GetAllAsync()
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync("api/servicerequests");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ServiceRequestApiDTO>>(json, JsonOpts) ?? new();
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
                var response = await client.GetAsync($"api/servicerequests/bycontract/{contractId}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ServiceRequestApiDTO>>(json, JsonOpts) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiServiceRequestService] GetByContract failed: {ex.Message}");
                return new();
            }
        }

        public async Task<(ServiceRequestApiDTO? _dto, string? error)> CreateAsync(CreateServiceRequestApiDTO _dto)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var content = new StringContent(JsonSerializer.Serialize(_dto), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/servicerequests", content);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    return (null, $"API error: {body}");
                }

                var json = await response.Content.ReadAsStringAsync();
                return (JsonSerializer.Deserialize<ServiceRequestApiDTO >(json, JsonOpts), null);
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
                var payload = new StringContent(JsonSerializer.Serialize(new { status }), Encoding.UTF8, "application/json");
                var response = await client.PatchAsync($"api/servicerequests/{id}/status", payload);
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
    //These 2 mirror the API's ServiceRequestDTO and CreateServiceRequestDTO, but are separate to avoid coupling the MVC project to the API's internal models.
    //This way if the API changes its internal models, we only need to update these DTOs and the ApiServiceRequestService, without affecting the rest of the MVC project.
    public class ServiceRequestApiDTO
    {
        public int RequestID { get; set; }
        public int ContractID { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? DocumentPath { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateServiceRequestApiDTO
    {
        public int ContractID { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal CostUSD { get; set; }
        public string? DocumentPath { get; set; }
    }
}
