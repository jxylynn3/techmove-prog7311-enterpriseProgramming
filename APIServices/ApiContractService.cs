using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ST10448420_TechMove_GLMS.ApiServices
{
    // This class replaces all direct _context.Contracts queries in the MVC project.
    // It calls the backend Web API over HTTP using IHttpClientFactory.
    // The MVC frontend NEVER touches the database directly after this refactor.
    public class ApiContractService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiContractService(IHttpClientFactory factory, IHttpContextAccessor accessor)
        {
            _httpClientFactory = factory;
            _httpContextAccessor = accessor;
        }

        // Creates an HttpClient with the JWT token attached if the user is logged in
        private HttpClient CreateAuthenticatedClient()
        {
            var client = _httpClientFactory.CreateClient("GlmsApi");
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public async Task<List<ContractApiDTO>> GetAllContractsAsync()
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync("api/contracts");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ContractApiDTO>>(json, JsonOpts) ?? new();
            }
            catch (Exception ex)
            {
                // If the API is down, return empty list — the view will show a friendly message
                Console.WriteLine($"[ApiContractService] GetAll failed: {ex.Message}");
                return new List<ContractApiDTO>();
            }
        }

        public async Task<ContractApiDTO?> GetContractByIdAsync(int id)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var response = await client.GetAsync($"api/contracts/{id}");
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ContractApiDTO>(json, JsonOpts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApiContractService] GetById({id}) failed: {ex.Message}");
                return null;
            }
        }

        public async Task<(ContractApiDTO? contract, string? error)> CreateContractAsync(CreateContractApiDTO _dto)
        {
            try
            {
                var client = CreateAuthenticatedClient();
                var content = new StringContent(JsonSerializer.Serialize(_dto), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/contracts", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    return (null, $"API error {(int)response.StatusCode}: {errorBody}");
                }

                var json = await response.Content.ReadAsStringAsync();
                return (JsonSerializer.Deserialize<ContractApiDTO>(json, JsonOpts), null);
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
                var response = await client.PatchAsync($"api/contracts/{id}/status", payload);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return (false, $"Contract {id} not found.");

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

    //this will be used by the MVC to display the contract data,while also making it easier to deserialize the API responses.
    //It also allows us to decouple the API's internal data model from what the MVC needs, which is good for flexibility and security.
    public class ContractApiDTO
    {//same as the ContractDTO in the API project, but with ClientName added for easier display in the MVC views without extra API calls.
        public int ContractID { get; set; }
        public int ClientID { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ServiceLevel { get; set; } = string.Empty;
        public string SignedAgreementFilePath { get; set; } = string.Empty;
    }

    public class CreateContractApiDTO
    {//same as internal ContractCreateDTO in the API project, but without the ClientName since it's not needed for creation and will be ignored by the API anyway.
        public int ClientID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Draft";
        public string ServiceLevel { get; set; } = string.Empty;
        public string SignedAgreementFilePath { get; set; } = string.Empty;
    }
}