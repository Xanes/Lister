using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    public class DeviceAuthService : IDeviceAuthService
    {
        private readonly ListerDbContext _context;

        public DeviceAuthService(ListerDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsDeviceTrustedAsync(string macAddress)
        {
            if (string.IsNullOrEmpty(macAddress))
                return false;

            return await _context.TrustedDevices
                .AnyAsync(d => d.Mac == macAddress);
        }

        public async Task<bool> AuthenticateWithPasswordAsync(string password, string macAddress)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(macAddress))
                return false;
            
            var passwordConfig = await _context.PasswordConfigs.FirstOrDefaultAsync();
            
            // If no password is set yet, hash and store the password 
            // (first-time setup)
            if (passwordConfig == null)
            {
                // Hash the password using MD5 on first setup
                var hashedPassword = ComputeMd5Hash(password);
                
                passwordConfig = new PasswordConfig { 
                    Password = hashedPassword,
                    LastModified = DateTime.UtcNow
                };
                _context.PasswordConfigs.Add(passwordConfig);
                await _context.SaveChangesAsync();
                
                // Add this device as trusted
                await AddTrustedDeviceAsync(macAddress);
                return true;
            }

            // For all subsequent logins, the client will send a pre-hashed password
            // so we just compare directly
            if (passwordConfig.Password == password)
            {
                // Add this device as trusted
                await AddTrustedDeviceAsync(macAddress);
                return true;
            }

            return false;
        }

        public async Task<bool> AddTrustedDeviceAsync(string macAddress)
        {
            if (string.IsNullOrEmpty(macAddress))
                return false;

            // Check if device already exists
            if (await _context.TrustedDevices.AnyAsync(d => d.Mac == macAddress))
                return true;

            // Add the new trusted device
            var device = new TrustedDevice
            {
                Mac = macAddress,
                CreatedAt = DateTime.UtcNow
            };

            _context.TrustedDevices.Add(device);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveTrustedDeviceAsync(string macAddress)
        {
            if (string.IsNullOrEmpty(macAddress))
                return false;

            var device = await _context.TrustedDevices
                .FirstOrDefaultAsync(d => d.Mac == macAddress);

            if (device == null)
                return false;

            _context.TrustedDevices.Remove(device);
            await _context.SaveChangesAsync();
            return true;
        }
        
        // Helper method to compute MD5 hash
        private string ComputeMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
} 