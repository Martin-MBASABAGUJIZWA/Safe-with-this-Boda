using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace SafeBoda.Admin.Services
{
    public class AuthService
    {
        private readonly ApiClient _apiClient;
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _js;

        private string? _token;
        private const string TokenKey = "authToken";

        public AuthService(ApiClient apiClient, HttpClient httpClient, IJSRuntime js)
        {
            _apiClient = apiClient;
            _httpClient = httpClient;
            _js = js;
        }

        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

        public async Task InitializeAsync()
        {
            // Try to get token from localStorage
            _token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
            if (!string.IsNullOrEmpty(_token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _token);
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var response = await _apiClient.LoginAsync(email, password);
            if (response == null || string.IsNullOrWhiteSpace(response.token))
            {
                _token = null;
                _httpClient.DefaultRequestHeaders.Authorization = null;
                await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
                return false;
            }

            _token = response.token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);

            // Save token to localStorage
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, _token);

            return true;
        }

        public async Task LogoutAsync()
        {
            _token = null;
            _httpClient.DefaultRequestHeaders.Authorization = null;
            await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }
    }
}
