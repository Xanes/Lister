using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using VerifyXunit;
using Xunit;

namespace IntegrationTests
{
    public class PdfControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>, IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly IServiceScopeFactory _scopeFactory;
        private Func<Task> _resetDatabase;
        private readonly string _testDataBasePath;

        public PdfControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _resetDatabase = InitializeDatabaseAsync;
            _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
            var assemblyPath = Path.GetDirectoryName(typeof(PdfControllerTests).Assembly.Location);
            _testDataBasePath = Path.Combine(assemblyPath ?? "", "TestDataFiles");
        }

        // --- Test Data --- 
        public static IEnumerable<object[]> PdfTestData()
        {
            for (int i = 1; i <= 4; i++) // Iterate through TestData_1 to TestData_4
            {
                 yield return new object[] { i }; // Pass folder index 
            }
        }

        // --- IAsyncLifetime Implementation for DB Reset --- 
        public async Task InitializeAsync()
        {
            await _resetDatabase();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        private async Task InitializeDatabaseAsync()
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ListerDbContext>();
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            // Seed necessary data if PdfController depends on it (e.g., password/device)
            // For now, assume only clean DB is needed for ReadPdf test
        }

        // --- Helper to ensure device is trusted --- 
        private async Task EnsureDeviceIsTrustedAsync(string macAddress)
        {
            // Add the device to the database if it doesn't exist
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ListerDbContext>();
                var deviceExists = await context.TrustedDevices.AnyAsync(d => d.Mac == macAddress);
                if (!deviceExists)
                {
                    context.TrustedDevices.Add(new Domain.Models.TrustedDevice { Mac = macAddress });
                    await context.SaveChangesAsync();
                }
            }

            // Add the X-Device-Mac header for the subsequent request
             _client.DefaultRequestHeaders.Remove("X-Device-Mac"); // Clear previous header if any
             _client.DefaultRequestHeaders.Add("X-Device-Mac", macAddress);
        }

        // --- Test Method --- 
        [Theory]
        [MemberData(nameof(PdfTestData))]
        public async Task ReadPdf_ReturnsVerifiedResult(int folderIndex)
        {
            // Arrange
            var deviceMac = $"PDF-TEST-{folderIndex}"; // Unique MAC for test isolation
            await EnsureDeviceIsTrustedAsync(deviceMac);

            // Adjust the path to be relative to the output directory
            var testDataFolder = Path.Combine(_testDataBasePath, $"TestData_{folderIndex}");
            var pdfPath1 = Path.Combine(testDataFolder, "1.pdf");
            var pdfPath2 = Path.Combine(testDataFolder, "2.pdf");

            if (!File.Exists(pdfPath1) || !File.Exists(pdfPath2))
            {
                throw new FileNotFoundException($"Test PDF files not found in {testDataFolder}");
            }

            using var formData = new MultipartFormDataContent();
            
            // Add files
            await using var fileStream1 = File.OpenRead(pdfPath1);
            var fileContent1 = new StreamContent(fileStream1);
            fileContent1.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            formData.Add(fileContent1, "fileList", Path.GetFileName(pdfPath1));
            
            await using var fileStream2 = File.OpenRead(pdfPath2);
            var fileContent2 = new StreamContent(fileStream2);
            fileContent2.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            formData.Add(fileContent2, "fileList", Path.GetFileName(pdfPath2));

            // Act
            // Construct URL with query parameters
            var nameParam = Uri.EscapeDataString("Test Shopping List " + folderIndex);
            var descriptionParam = Uri.EscapeDataString("Generated by integration test");
            var requestUrl = $"api/Pdf/ReadPdf?name={nameParam}&description={descriptionParam}";
            var response = await _client.PostAsync(requestUrl, formData);

            // Assert
            response.EnsureSuccessStatusCode(); // Check for 2xx status code
            var responseData = await response.Content.ReadAsStringAsync(); // Read as string for Verify

            // Use Verify to snapshot the JSON string response
            await Verifier.VerifyJson(responseData)
                          .UseDirectory("VerifiedSnapshots") // Optional: customize snapshot directory
                          .UseParameters(folderIndex); // Creates separate snapshot files per folder index
        }
    }
} 