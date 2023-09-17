using Domain.Interfaces;
using Domain.Models;
using Infrastructure.Database;
using Infrastructure.Repositories;
using Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ListerDbContext>(options =>
{
    var DbConnectionString = builder.Configuration["DbConnectionString"];
    var version = builder.Configuration["MySqlVersion"];
    options.UseMySql(DbConnectionString, new MySqlServerVersion(new Version(version)), b => b.MigrationsAssembly("Infrastructure"));
}
);

builder.Services.AddSingleton<ISettings, AvoDietSettings>();
builder.Services.AddTransient<IRepository<ShoppingList>, ShoppingListRepository>();

var app = builder.Build();
app.MapGet("/", () => StatusCodes.Status200OK);
app.UseCors(op =>
{
    op.AllowAnyHeader();
    op.AllowAnyMethod();
    op.AllowAnyOrigin();
});
app.UseRouting();
app.UseSwagger();
app.UseSwaggerUI();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();