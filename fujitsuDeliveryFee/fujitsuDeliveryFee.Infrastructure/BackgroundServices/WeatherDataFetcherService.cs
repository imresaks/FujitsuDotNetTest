using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using fujitsuDeliveryFee.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace fujitsuDeliveryFee.Infrastructure.BackgroundServices
{
    /// <summary>
    /// Background service for fetching weather data periodically
    /// </summary>
    public class WeatherDataFetcherService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<WeatherDataFetcherService> _logger;
        private readonly string _cronExpression;

        /// <summary>
        /// Initializes a new instance of the WeatherDataFetcherService class
        /// </summary>
        /// <param name="serviceProvider">The service provider</param>
        /// <param name="logger">The logger</param>
        /// <param name="configuration">The configuration</param>
        public WeatherDataFetcherService(
            IServiceProvider serviceProvider,
            ILogger<WeatherDataFetcherService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            
            // Default cron expression: "15 * * * *" (15 minutes after every hour)
            _cronExpression = configuration.GetValue<string>("WeatherDataFetcher:CronExpression") ?? "15 * * * *";
            
            _logger.LogInformation("Weather data fetcher service initialized with cron expression: {CronExpression}", _cronExpression);
        }

        /// <summary>
        /// Executes the background service
        /// </summary>
        /// <param name="stoppingToken">The cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Weather data fetcher service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                var nextRunTime = GetNextRunTime();
                var delay = nextRunTime - DateTime.UtcNow;

                if (delay.TotalMilliseconds <= 0)
                {   
                    // If we're past the scheduled time, calculate the next run time
                    nextRunTime = GetNextRunTime(DateTime.UtcNow.AddMinutes(1));
                    delay = nextRunTime - DateTime.UtcNow;
                }

                _logger.LogInformation("Next weather data fetch scheduled at: {NextRunTime} (in {Delay})", 
                    nextRunTime.ToLocalTime(), delay);

                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {   
                    try
                    {   
                        await FetchWeatherDataAsync();
                    }
                    catch (Exception ex)
                    {   
                        _logger.LogError(ex, "Error occurred while fetching weather data");
                    }
                }
            }

            _logger.LogInformation("Weather data fetcher service is stopping");
        }

        /// <summary>
        /// Fetches weather data from the Estonian Environment Agency
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task FetchWeatherDataAsync()
        {
            _logger.LogInformation("Fetching weather data");

            using var scope = _serviceProvider.CreateScope();
            var weatherDataService = scope.ServiceProvider.GetRequiredService<IWeatherDataService>();

            try
            {
                var weatherData = await weatherDataService.FetchWeatherDataAsync();
                await weatherDataService.SaveWeatherDataAsync(weatherData);
                
                _logger.LogInformation("Successfully fetched and saved weather data");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching or saving weather data");
                throw;
            }
        }

        /// <summary>
        /// Gets the next run time based on the cron expression
        /// </summary>
        /// <param name="fromUtc">The starting time (defaults to current UTC time)</param>
        /// <returns>The next run time</returns>
        private DateTime GetNextRunTime(DateTime? fromUtc = null)
        {
            fromUtc ??= DateTime.UtcNow;
            var cronExpression = CronExpression.Parse(_cronExpression);
            var nextRun = cronExpression.GetNextOccurrence(fromUtc.Value, TimeZoneInfo.Utc);
            
            return nextRun ?? fromUtc.Value.AddHours(1); // Fallback to hourly if parsing fails
        }
    }
}