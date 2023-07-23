var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
var app = builder.Build();
app.MapGet("/", () => "Hello World!");
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
