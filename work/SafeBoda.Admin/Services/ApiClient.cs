using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SafeBoda.Admin.Services
{
    // ============ REQUEST/RESPONSE MODELS ============
    
    public class LoginRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string token { get; set; } = string.Empty;
    }

    // Location and Trip records (for API responses)
    public record Location(double Latitude, double Longitude);

    public record Trip(
        Guid Id,
        Guid RiderId,
        Guid DriverId,
        Location Start,
        Location End,
        decimal Fare,
        DateTime RequestTime
    );

    public record Rider(
        Guid Id,
        string Name,
        string PhoneNumber
    );

    public record Driver(
        Guid Id,
        string Name,
        string PhoneNumber,
        string MotoPlateNumber
    );

    // Update DTOs for API requests
    public class UpdateRiderRequest
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class UpdateDriverRequest
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string MotoPlateNumber { get; set; } = string.Empty;
    }

    // ============ API CLIENT ============

    public class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ---- AUTHENTICATION ----
        public async Task<LoginResponse?> LoginAsync(string email, string password)
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        // ---- TRIPS ----
        public async Task<IEnumerable<Trip>?> GetTripsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Trip>>("api/trips");
        }

        public async Task<Trip?> GetTripByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Trip>($"api/trips/{id}");
        }

        public async Task<Trip?> CreateTripAsync(Trip trip)
        {
            var response = await _httpClient.PostAsJsonAsync("api/trips", trip);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Trip>();
        }

        public async Task<bool> UpdateTripAsync(Guid id, Trip trip)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/trips/{id}", trip);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteTripAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/trips/{id}");
            return response.IsSuccessStatusCode;
        }

        // ---- RIDERS ----
        public async Task<IEnumerable<Rider>?> GetRidersAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Rider>>("api/riders");
        }

        public async Task<Rider?> GetRiderByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Rider>($"api/riders/{id}");
        }

        public async Task<Rider?> CreateRiderAsync(Rider rider)
        {
            var response = await _httpClient.PostAsJsonAsync("api/riders", rider);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Rider>();
        }

        public async Task<bool> UpdateRiderAsync(Guid id, UpdateRiderRequest request)
        {
            var rider = new { Id = id, Name = request.Name, PhoneNumber = request.PhoneNumber };
            var response = await _httpClient.PutAsJsonAsync($"api/riders/{id}", rider);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRiderAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/riders/{id}");
            return response.IsSuccessStatusCode;
        }

        // ---- DRIVERS ----
        public async Task<IEnumerable<Driver>?> GetDriversAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Driver>>("api/drivers");
        }

        public async Task<Driver?> GetDriverByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Driver>($"api/drivers/{id}");
        }

        public async Task<Driver?> CreateDriverAsync(Driver driver)
        {
            var response = await _httpClient.PostAsJsonAsync("api/drivers", driver);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Driver>();
        }

        public async Task<bool> UpdateDriverAsync(Guid id, UpdateDriverRequest request)
        {
            var driver = new { Id = id, Name = request.Name, PhoneNumber = request.PhoneNumber, MotoPlateNumber = request.MotoPlateNumber };
            var response = await _httpClient.PutAsJsonAsync($"api/drivers/{id}", driver);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDriverAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/drivers/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}