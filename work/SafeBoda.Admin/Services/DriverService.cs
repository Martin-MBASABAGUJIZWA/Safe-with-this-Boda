using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SafeBoda.Admin.Services
{
    // DTO wrapper for display
    public record DriverDto(Guid Id, string Name, string PhoneNumber, string MotoPlateNumber);

    public class DriverService
    {
        private readonly ApiClient _apiClient;

        public DriverService(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IEnumerable<DriverDto>?> GetAllDriversAsync()
        {
            try
            {
                var drivers = await _apiClient.GetDriversAsync();
                return drivers?.Select(d => new DriverDto(d.Id, d.Name, d.PhoneNumber, d.MotoPlateNumber)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading drivers: {ex.Message}");
                return null;
            }
        }

        public async Task<DriverDto?> GetDriverAsync(Guid id)
        {
            try
            {
                var driver = await _apiClient.GetDriverByIdAsync(id);
                return driver != null ? new DriverDto(driver.Id, driver.Name, driver.PhoneNumber, driver.MotoPlateNumber) : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading driver: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateDriverAsync(string name, string phoneNumber, string motoPlateNumber)
        {
            try
            {
                var driver = new Driver(Guid.NewGuid(), name, phoneNumber, motoPlateNumber);
                var result = await _apiClient.CreateDriverAsync(driver);
                return result != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateDriverAsync(Guid id, string name, string phoneNumber, string motoPlateNumber)
        {
            try
            {
                var request = new UpdateDriverRequest { Name = name, PhoneNumber = phoneNumber, MotoPlateNumber = motoPlateNumber };
                return await _apiClient.UpdateDriverAsync(id, request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteDriverAsync(Guid id)
        {
            try
            {
                return await _apiClient.DeleteDriverAsync(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
