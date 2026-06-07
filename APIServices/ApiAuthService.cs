using System.Text;
using System.Text.Json;

namespace ST10448420_TechMove_GLMS.ApiServices
{
    // Handles JWT login: calls POST /api/auth/login and returns the token + roles.
    // The token is stored in session by AccountController after a successful login.
    public class ApiAuthService
    {
        private readonly IHttpClientFactory _factory;

        public ApiAuthService(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        public async Task<(string? token, List<string>? roles, string? error)> LoginAsync(
            string email, string password)
        {
            try
            {
                var client = _factory.CreateClient("GlmsApi");
                var body = new StringContent(
                    JsonSerializer.Serialize(new { email, password }),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync("api/auth/login", body);

                if (!response.IsSuccessStatusCode)
                    return (null, null, "Invalid email or password.");

                var json = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<LoginResponse>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return (data?.Token, data?.Roles, null);
            }
            catch (Exception ex)
            {
                return (null, null,
                    $"Cannot reach authentication service: {ex.Message}");
            }
        }

        // Private class — only used to deserialize the /api/auth/login JSON response
        private class LoginResponse
        {
            public string? Token { get; set; }
            public List<string>? Roles { get; set; }
        }
    }
}