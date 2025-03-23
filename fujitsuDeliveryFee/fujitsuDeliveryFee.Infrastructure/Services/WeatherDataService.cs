using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using fujitsuDeliveryFee.Domain.Entities;
using fujitsuDeliveryFee.Domain.Interfaces;
using fujitsuDeliveryFee.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace fujitsuDeliveryFee.Infrastructure.Services
{
    /// <summary>
    /// Service for fetching and managing weather data
    /// </summary>
    public class WeatherDataService : IWeatherDataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<WeatherDataService> _logger;
        private readonly string _weatherDataUrl = "https://www.ilmateenistus.ee/ilma_andmed/xml/observations.php";

        // Dictionary mapping station names to city names
        private readonly Dictionary<string, string> _stationToCity = new Dictionary<string, string>
        {
            { "Tallinn-Harku", "Tallinn" },
            { "Tartu-Tõravere", "Tartu" },
            { "Pärnu", "Pärnu" }
        };

        /// <summary>
        /// Initializes a new instance of the WeatherDataService class
        /// </summary>
        /// <param name="dbContext">The database context</param>
        /// <param name="httpClientFactory">The HTTP client factory</param>
        /// <param name="logger">The logger</param>
        public WeatherDataService(
            ApplicationDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            ILogger<WeatherDataService> logger)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Fetches the latest weather data from the Estonian Environment Agency
        /// </summary>
        /// <returns>A collection of weather data objects</returns>
        public async Task<IEnumerable<WeatherData>> FetchWeatherDataAsync()
        {
            try
            {
                _logger.LogInformation("Fetching weather data from {Url}", _weatherDataUrl);
                
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync(_weatherDataUrl);
                
                var weatherData = ParseWeatherData(response);
                
                _logger.LogInformation("Successfully fetched {Count} weather data records", weatherData.Count());
                
                return weatherData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching weather data from {Url}", _weatherDataUrl);
                throw;
            }
        }

        /// <summary>
        /// Gets the latest weather data for a specific city
        /// </summary>
        /// <param name="city">The name of the city</param>
        /// <returns>The latest weather data for the specified city</returns>
        public async Task<WeatherData?> GetLatestWeatherDataForCityAsync(string city)
        {
            try
            {
                return await _dbContext.WeatherData
                    .Where(w => w.City == city)
                    .OrderByDescending(w => w.Timestamp)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest weather data for city {City}", city);
                throw;
            }
        }

        /// <summary>
        /// Saves weather data to the database
        /// </summary>
        /// <param name="weatherData">The weather data to save</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SaveWeatherDataAsync(IEnumerable<WeatherData> weatherData)
        {
            try
            {
                await _dbContext.WeatherData.AddRangeAsync(weatherData);
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Successfully saved {Count} weather data records", weatherData.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving weather data");
                throw;
            }
        }

        /// <summary>
        /// Parses the XML response from the weather service into WeatherData objects
        /// </summary>
        /// <param name="xmlResponse">The XML response from the weather service</param>
        /// <returns>A collection of WeatherData objects</returns>
        private IEnumerable<WeatherData> ParseWeatherData(string xmlResponse)
        {
            var result = new List<WeatherData>();
            var xDoc = XDocument.Parse(xmlResponse);
            var timestamp = DateTime.Parse(xDoc.Root?.Attribute("timestamp")?.Value ?? DateTime.Now.ToString());

            var stations = xDoc.Descendants("station");
            
            foreach (var station in stations)
            {
                var stationName = station.Element("name")?.Value;
                
                // Only process stations we're interested in
                if (stationName != null && _stationToCity.ContainsKey(stationName))
                {
                    var wmoCode = station.Element("wmocode")?.Value ?? string.Empty;
                    var airTemperature = decimal.TryParse(station.Element("airtemperature")?.Value, out var temp) ? temp : 0;
                    var windSpeed = decimal.TryParse(station.Element("windspeed")?.Value, out var wind) ? wind : 0;
                    var phenomenon = station.Element("phenomenon")?.Value ?? string.Empty;
                    
                    var weatherData = new WeatherData
                    {
                        StationName = stationName,
                        WmoCode = wmoCode,
                        AirTemperature = airTemperature,
                        WindSpeed = windSpeed,
                        WeatherPhenomenon = phenomenon,
                        Timestamp = timestamp,
                        City = _stationToCity[stationName]
                    };
                    
                    result.Add(weatherData);
                }
            }
            
            return result;
        }
    }
}