using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

try
{
    var config = new ConfigurationBuilder().AddJsonFile("appsettings.Logs.json").Build();

    Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(config)
                   .MinimumLevel.Debug()
                   .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                   .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
                   .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                   .CreateLogger();

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();
    Log.Information("Starting Server At : {V}", DateTime.Now.ToString());

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();    // Maps the /scalar route
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    // Minimal Health Check API endpoint
    app.MapGet("/HealthCheck", (IConfiguration config) =>
    {
        return Results.Ok(new
        {
            Author = "RP-CICD",
            Environment = app.Environment.EnvironmentName,
            Status = "Healthy",
            TimestampUtc = DateTime.UtcNow.ToLocalTime()
        });
    }).WithName("HealthCheck");


    Log.Information("Started Server At : {V}", DateTime.Now.ToString());

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "The server failed to start : {V}", DateTime.Now.ToString());
}
finally
{
    Log.CloseAndFlush();
}