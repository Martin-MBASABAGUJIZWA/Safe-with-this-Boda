using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static SafeBoda.Admin.Services.ApiClient;

namespace SafeBoda.Admin.Services
{
    public class TripService
    {
        private readonly ApiClient _apiClient;

        public TripService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<Trip>?> GetAllTripsAsync()
        {
            try
            {
                return await _apiClient.GetTripsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading trips: {ex.Message}");
                return null;
            }
        }

        public async Task<Trip?> GetTripAsync(Guid id)
        {
            try
            {
                return await _apiClient.GetTripByIdAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading trip: {ex.Message}");
                return null;
            }
        }

        public async Task<Trip?> CreateTripAsync(Trip trip)
        {
            try
            {
                return await _apiClient.CreateTripAsync(trip);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating trip: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateTripAsync(Guid id, Trip trip)
        {
            try
            {
                return await _apiClient.UpdateTripAsync(id, trip);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating trip: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTripAsync(Guid id)
        {
            try
            {
                return await _apiClient.DeleteTripAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting trip: {ex.Message}");
                return false;
            }
        }
    }
}
