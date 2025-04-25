using Domain.Models;
using Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Utils
{
    public static class DbContextSeeds
    {
        public static async Task AddPasswordToDb(this CustomWebApplicationFactory<Program> factory, string password)
        {
            using (var scope = factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ListerDbContext>();
                if (!string.IsNullOrEmpty(password)) // Only seed if you calculated it
                {
                    context.PasswordConfigs.Add(new PasswordConfig
                    {
                        Password = password.ComputeMd5Hash(), // Use the pre-calculated hash
                        LastModified = DateTime.UtcNow
                    });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
