using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafeBoda.Admin.Services
{
    // DTO wrapper for display
    public record RiderDto(Guid Id, string Name, string PhoneNumber);

    public class RiderService
    {
        private readonly ApiClient _apiClient;

        public RiderService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<RiderDto>?> GetAllRidersAsync()
        {
            try
            {
                var riders = await _apiClient.GetRidersAsync();
                return riders?.Select(r => new RiderDto(r.Id, r.Name, r.PhoneNumber)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading riders: {ex.Message}");
                return null;
            }
        }

        public async Task<RiderDto?> GetRiderAsync(Guid id)
        {
            try
            {
                var rider = await _apiClient.GetRiderByIdAsync(id);
                return rider != null ? new RiderDto(rider.Id, rider.Name, rider.PhoneNumber) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading rider: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateRiderAsync(string name, string phoneNumber)
        {
            try
            {
                var rider = new Rider(Guid.NewGuid(), name, phoneNumber);
                var result = await _apiClient.CreateRiderAsync(rider);
                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateRiderAsync(Guid id, string name, string phoneNumber)
        {
            try
            {
                var request = new UpdateRiderRequest { Name = name, PhoneNumber = phoneNumber };
                return await _apiClient.UpdateRiderAsync(id, request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteRiderAsync(Guid id)
        {
            try
            {
                return await _apiClient.DeleteRiderAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
