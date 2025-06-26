using Healink.Application;
using Healink.Infrastructure;
using Healink.API.Configurations;
using Healink.API.Injection;

try
{
    Console.WriteLine("Starting Healink API...");

    var builder = WebApplication.CreateBuilder(args);

    // Add logging configuration first
    try
    {
        builder.AddLoggingConfiguration();
        Console.WriteLine("✓ Logging configuration loaded");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ Warning: Logging configuration failed: {ex.Message}");
    }

    // Load environment variables
    try
    {
        builder.AddEnvironmentConfiguration();
        Console.WriteLine("✓ Environment configuration loaded");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ Warning: Environment configuration failed: {ex.Message}");
    }

    // Add services to the container
    builder.Services.AddControllers();
    Console.WriteLine("✓ Controllers added");

    // Add Clean Architecture layers
    try
    {
        builder.Services.AddApplication();
        Console.WriteLine("✓ Application layer added");
        
        builder.Services.AddInfrastructure(builder.Configuration);
        Console.WriteLine("✓ Infrastructure layer added");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error adding architecture layers: {ex.Message}");
        throw;
    }

    // Add API-specific services
    try
    {
        builder.Services.AddApiServices(builder.Configuration);
        Console.WriteLine("✓ API services added");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ Warning: API services failed: {ex.Message}");
    }

    // Add Swagger configuration
    try
    {
        builder.Services.AddSwaggerConfiguration();
        Console.WriteLine("✓ Swagger configuration added");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ Warning: Swagger configuration failed: {ex.Message}");
    }

    // Add CORS configuration
    try
    {
        builder.Services.AddCorsConfiguration(builder.Configuration);
        Console.WriteLine("✓ CORS configuration added");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ Warning: CORS configuration failed: {ex.Message}");
    }

    // Add JWT configuration
    try
    {
        var requireHttps = builder.Configuration.GetValue<bool>("Security:RequireHttps");
        builder.Services.AddJwtConfiguration(builder.Configuration, requireHttps);
        Console.WriteLine("✓ JWT configuration added");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ Warning: JWT configuration failed: {ex.Message}");
    }

    // Add endpoints API explorer
    builder.Services.AddEndpointsApiExplorer();
    Console.WriteLine("✓ API explorer added");

    var app = builder.Build();
    Console.WriteLine("✓ Application built successfully");

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        try
        {
            app.UseSwaggerConfiguration();
            Console.WriteLine("✓ Swagger configured for development");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠ Warning: Swagger configuration failed: {ex.Message}");
        }
    }

    // Use HTTPS redirection only in production
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    // Use static files for wwwroot
    try
    {
        app.UseStaticFiles();
        Console.WriteLine("✓ Static files configured");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ Warning: Static files failed: {ex.Message}");
    }

    // Use CORS configuration
    try
    {
        app.UseCorsConfiguration();
        Console.WriteLine("✓ CORS configured");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ Warning: CORS configuration failed: {ex.Message}");
    }

    // Use API configuration (includes global exception handling and JWT middleware)
    try
    {
        app.UseApiConfiguration();
        Console.WriteLine("✓ API configuration applied");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠ Warning: API configuration failed: {ex.Message}");
    }

    // Use authentication and authorization
    app.UseAuthentication();
    app.UseAuthorization();
    Console.WriteLine("✓ Authentication and authorization configured");

    // Map controllers
    app.MapControllers();
    Console.WriteLine("✓ Controllers mapped");

    Console.WriteLine("🚀 Healink API starting on ports:");
    Console.WriteLine($"   - HTTP:  http://localhost:5000");
    Console.WriteLine($"   - HTTPS: https://localhost:5001");
    Console.WriteLine($"   - Swagger: http://localhost:5000/swagger");
    Console.WriteLine();

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Fatal error starting Healink API: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    Environment.Exit(-1);
}
