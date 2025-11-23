using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Database;
using Infrastructure.OpenAI.Interfaces;
using Infrastructure.OpenAI.Settings;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PDFReader.Extensions;
using PDFReader.Middleware;

// Define a public class Program
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        // Configure Swagger
        builder.UseSwagger();

        builder.Services.AddDbContext<ListerDbContext>(options =>
        {
            var DbConnectionString = builder.Configuration["DbConnectionString"];
            string version = builder.Configuration["MySqlVersion"] ?? "";
            options.UseMySql(DbConnectionString, new MySqlServerVersion(new Version(version)), b => b.MigrationsAssembly("Infrastructure"));
        }
        );

        builder.Services.AddSingleton<ISettings, AvoDietSettings>();
        // Register repositories
        builder.Services.AddTransient<IDietRepository, DietRepository>();
        builder.Services.AddTransient<IReadOnlyRepository<ProductsDescriptionInfo>, ProductsDescriptionInfoRepository>();
        builder.Services.AddTransient<IAdditionalProductRepository, AdditionalProductRepository>();
        builder.Services.AddScoped<IDeviceAuthService, DeviceAuthService>();
        builder.Services.AddTransient<IOpenAIRepository, OpenAIRepository>();
        builder.Services.AddTransient<IOpenAIService, OpenAiService>();
        builder.Services.Configure<OpenAiSettings>(builder.Configuration.GetSection("OpenAI"));
        builder.Services.AddHttpClient();

        var app = builder.Build();
        app.MapGet("/", () => StatusCodes.Status200OK);
        app.UseCors(op =>
        {
            op.AllowAnyHeader();
            op.AllowAnyMethod();
            op.AllowAnyOrigin();
        });
        app.UseRouting();

        // Add device authentication middleware
        app.UseDeviceAuthentication();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
        app.Run();
    }
}

// Remove the previous partial class marker
// public partial class Program { }

