using Infrastructure.Database;
using IntegrationTests.Models;
using IntegrationTests.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PDFReader.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime // Implement IAsyncLifetime for seeding
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        // Removed: private readonly IDeviceAuthService _deviceAuthServiceSubstitute;
        private Func<Task> _resetDatabase; // Delegate to reset DB

        public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            // Removed: _deviceAuthServiceSubstitute = _factory.DeviceAuthServiceSubstitute;
            _resetDatabase = InitializeDatabaseAsync; // Assign reset delegate
        }

        // Seed data before tests run
        public async Task InitializeAsync()
        {
            await _resetDatabase(); // Call reset before each test run starts (via delegate)
        }

        // Clean up after tests (optional, as in-memory DB is often recreated)
        public Task DisposeAsync()
        {
            // Can add cleanup here if needed
            return Task.CompletedTask;
        }

        // Helper to seed the database
        private async Task InitializeDatabaseAsync()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ListerDbContext>();

                // Ensure clean state for each test
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
            }
        }

        [Fact]
        public async Task Login_WithFirstTimeCorrectPassword_ReturnsOkAndAddsDevice()
        {
            // Arrange: Assuming no password is set initially
            var loginRequest = new LoginRequest
            {
                Password = "firstTimePassword", // Use the password that will be hashed
                DeviceMac = "AA:BB:CC:DD:EE:FF"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

            // Assert
            response.EnsureSuccessStatusCode(); // Check for 2xx status code
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(responseData);
            Assert.True(responseData.Trusted);
            Assert.Equal("Authentication successful", responseData.Message);

            // Verify device was added (optional, but good practice)
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ListerDbContext>();
                var device = await context.TrustedDevices.FirstOrDefaultAsync(t => t.Mac == loginRequest.DeviceMac);
                var password = await context.PasswordConfigs.FirstOrDefaultAsync(p => p.Password == loginRequest.Password.ComputeMd5Hash());
                Assert.NotNull(device);
                Assert.NotNull(password);
            }
        }

        [Fact]
        public async Task Login_WithExistingCorrectHash_ReturnsOk()
        {
            // Arrange: Ensure DB is seeded with the correct hash for this test
            // This relies on InitializeDatabaseAsync having seeded the correct hash
            var correctPassword = "Correct"; // TODO: Get the hash seeded in InitializeDatabaseAsync
            await _factory.AddPasswordToDb(correctPassword);
            var loginRequest = new LoginRequest
            {
                Password = correctPassword, // Send the pre-hashed password
                DeviceMac = "BB:CC:DD:EE:FF:00"
            };

            // Ensure the hash exists before the test
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ListerDbContext>();
                var config = await context.PasswordConfigs.FirstOrDefaultAsync();
                Assert.NotNull(config); // Make sure seeding worked
                correctPassword = config.Password; // Use the actual seeded hash
                loginRequest.Password = correctPassword;
            }

            // Act
            var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(responseData);
            Assert.True(responseData.Trusted);
        }

        [Fact]
        public async Task Login_WithInvalidPasswordOrHash_ReturnsUnauthorized()
        {
            // Arrange: Can run against initial empty state or seeded state
            await _factory.AddPasswordToDb("any");
            var loginRequest = new LoginRequest
            {
                Password = "incorrect_password_or_hash",
                DeviceMac = "CC:DD:EE:FF:00:11"
            };

            // Optional: Seed a password first if needed for this scenario
            // await InitializeDatabaseAsync(); // Or a specific seed for this test

            // Act
            var response = await _client.PostAsJsonAsync("/auth/login", loginRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            var responseData = await response.Content.ReadFromJsonAsync<LoginResponse>();
            Assert.NotNull(responseData);
            Assert.False(responseData.Trusted);
            Assert.Equal("Invalid password", responseData.Message);
        }


    }
}