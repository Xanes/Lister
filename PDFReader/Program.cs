using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Database;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using PDFReader.Extensions;
using PDFReader.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Configure Swagger
builder.UseSwagger();

builder.Services.AddDbContext<ListerDbContext>(options =>
{
    var DbConnectionString = builder.Configuration["DbConnectionString"];
    var version = builder.Configuration["MySqlVersion"];
    options.UseMySql(DbConnectionString, new MySqlServerVersion(new Version(version)), b => b.MigrationsAssembly("Infrastructure"));
}
);

builder.Services.AddSingleton<ISettings, AvoDietSettings>();
// Register repositories
builder.Services.AddTransient<IReadOnlyBulkRepository<ShoppingList>, ShoppingListRepository>();
builder.Services.AddTransient<IRepository<ShoppingList, ProductChange>, ShoppingListRepository>();
builder.Services.AddTransient<IReadOnlyRepository<ProductsDescriptionInfo>, ProductsDescriptionInfoRepository>();
builder.Services.AddScoped<IDeviceAuthService, DeviceAuthService>();

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

