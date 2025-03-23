using fujitsuDeliveryFee.Application.Services;
using fujitsuDeliveryFee.Domain.Interfaces;
using fujitsuDeliveryFee.Infrastructure.BackgroundServices;
using fujitsuDeliveryFee.Infrastructure.Data;
using fujitsuDeliveryFee.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fujitsu Delivery Fee API",
        Version = "v1",
        Description = "API for calculating delivery fees based on city, vehicle type, and weather conditions"
    });
    
    // Add XML comments to Swagger
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register HttpClient for weather data service
builder.Services.AddHttpClient();

// Register services
builder.Services.AddScoped<IWeatherDataService, WeatherDataService>();
builder.Services.AddScoped<IDeliveryFeeCalculationService, DeliveryFeeCalculationService>();

// Register background service for weather data fetching
builder.Services.AddHostedService<WeatherDataFetcherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
