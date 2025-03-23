using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IDeviceAuthService
    {
        Task<bool> IsDeviceTrustedAsync(string macAddress);
        Task<bool> AuthenticateWithPasswordAsync(string password, string macAddress);
        Task<bool> AddTrustedDeviceAsync(string macAddress);
        Task<bool> RemoveTrustedDeviceAsync(string macAddress);
    }
} 