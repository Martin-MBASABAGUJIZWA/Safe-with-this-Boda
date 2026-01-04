using SafeBoda.Core;
using System.Net.Http.Json;



public class TripService
{
    private readonly HttpClient _http;

    public TripService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Trip>> GetTripsAsync()
    {
        // Assuming API endpoint is /api/Trips
        var trips = await _http.GetFromJsonAsync<List<Trip>>("https://localhost:5228/api/Trips");

        // var trips = await _http.GetFromJsonAsync<List<Trip>>("");
        return trips ?? new List<Trip>();
    }
}
