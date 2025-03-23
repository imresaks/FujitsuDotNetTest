using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fujitsuDeliveryFee.Domain.Entities;
using fujitsuDeliveryFee.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace fujitsuDeliveryFee.Application.Services
{
    /// <summary>
    /// Service for calculating delivery fees based on city, vehicle type, and weather conditions
    /// </summary>
    public class DeliveryFeeCalculationService : IDeliveryFeeCalculationService
    {
        private readonly IWeatherDataService _weatherDataService;
        private readonly ILogger<DeliveryFeeCalculationService> _logger;

        // Dictionary for regional base fees by city and vehicle type
        private readonly Dictionary<string, Dictionary<string, decimal>> _regionalBaseFees = new Dictionary<string, Dictionary<string, decimal>>
        {
            { "Tallinn", new Dictionary<string, decimal>
                {
                    { "Car", 4.0m },
                    { "Scooter", 3.5m },
                    { "Bike", 3.0m }
                }
            },
            { "Tartu", new Dictionary<string, decimal>
                {
                    { "Car", 3.5m },
                    { "Scooter", 3.0m },
                    { "Bike", 2.5m }
                }
            },
            { "Pärnu", new Dictionary<string, decimal>
                {
                    { "Car", 3.0m },
                    { "Scooter", 2.5m },
                    { "Bike", 2.0m }
                }
            }
        };

        // List of weather phenomena related to snow or sleet
        private readonly List<string> _snowOrSleetPhenomena = new List<string>
        {
            "Light snow shower", "Moderate snow shower", "Heavy snow shower",
            "Light sleet", "Moderate sleet", "Light snowfall", "Moderate snowfall", "Heavy snowfall",
            "Blowing snow", "Drifting snow", "Snow", "Snow shower", "Sleet", "Snowfall"
        };

        // List of weather phenomena related to rain
        private readonly List<string> _rainPhenomena = new List<string>
        {
            "Light rain", "Moderate rain", "Heavy rain", "Light shower", "Moderate shower", "Heavy shower",
            "Rain", "Shower", "Light rain shower", "Moderate rain shower", "Heavy rain shower"
        };

        // List of forbidden weather phenomena for bikes and scooters
        private readonly List<string> _forbiddenPhenomena = new List<string>
        {
            "Glaze", "Hail", "Thunder", "Thunderstorm"
        };

        /// <summary>
        /// Initializes a new instance of the DeliveryFeeCalculationService class
        /// </summary>
        /// <param name="weatherDataService">The weather data service</param>
        /// <param name="logger">The logger</param>
        public DeliveryFeeCalculationService(
            IWeatherDataService weatherDataService,
            ILogger<DeliveryFeeCalculationService> logger)
        {
            _weatherDataService = weatherDataService;
            _logger = logger;
        }

        /// <summary>
        /// Calculates the delivery fee based on city and vehicle type
        /// </summary>
        /// <param name="city">The city for delivery (Tallinn, Tartu, Pärnu)</param>
        /// <param name="vehicleType">The vehicle type (Car, Scooter, Bike)</param>
        /// <returns>The calculated delivery fee or throws an exception if vehicle usage is forbidden</returns>
        public async Task<decimal> CalculateDeliveryFeeAsync(string city, string vehicleType)
        {
            _logger.LogInformation("Calculating delivery fee for city: {City}, vehicle type: {VehicleType}", city, vehicleType);

            // Validate input parameters
            if (!_regionalBaseFees.ContainsKey(city))
            {
                throw new ArgumentException($"Invalid city: {city}. Valid cities are: Tallinn, Tartu, Pärnu");
            }

            if (!_regionalBaseFees[city].ContainsKey(vehicleType))
            {
                throw new ArgumentException($"Invalid vehicle type: {vehicleType}. Valid vehicle types are: Car, Scooter, Bike");
            }

            // Get the latest weather data for the city
            var weatherData = await _weatherDataService.GetLatestWeatherDataForCityAsync(city);
            if (weatherData == null)
            {
                throw new InvalidOperationException($"No weather data available for city: {city}");
            }

            _logger.LogInformation("Latest weather data for {City}: Temperature: {Temperature}°C, Wind Speed: {WindSpeed} m/s, Phenomenon: {Phenomenon}",
                city, weatherData.AirTemperature, weatherData.WindSpeed, weatherData.WeatherPhenomenon);

            // Check for forbidden conditions
            CheckForbiddenConditions(vehicleType, weatherData);

            // Calculate the regional base fee
            decimal totalFee = _regionalBaseFees[city][vehicleType];
            _logger.LogInformation("Regional base fee: {Fee} €", totalFee);

            // Calculate extra fees based on weather conditions
            decimal extraFees = CalculateExtraFees(vehicleType, weatherData);
            _logger.LogInformation("Extra fees: {Fee} €", extraFees);

            // Calculate the total fee
            totalFee += extraFees;
            _logger.LogInformation("Total delivery fee: {Fee} €", totalFee);

            return totalFee;
        }

        /// <summary>
        /// Checks if the vehicle usage is forbidden based on weather conditions
        /// </summary>
        /// <param name="vehicleType">The vehicle type</param>
        /// <param name="weatherData">The weather data</param>
        private void CheckForbiddenConditions(string vehicleType, WeatherData weatherData)
        {
            // Check for forbidden wind speed for bikes
            if (vehicleType == "Bike" && weatherData.WindSpeed > 20.0m)
            {
                _logger.LogWarning("Usage of bike is forbidden due to high wind speed: {WindSpeed} m/s", weatherData.WindSpeed);
                throw new InvalidOperationException("Usage of selected vehicle type is forbidden");
            }

            // Check for forbidden weather phenomena for bikes and scooters
            if ((vehicleType == "Bike" || vehicleType == "Scooter") && 
                _forbiddenPhenomena.Any(p => weatherData.WeatherPhenomenon.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                _logger.LogWarning("Usage of {VehicleType} is forbidden due to weather phenomenon: {Phenomenon}", 
                    vehicleType, weatherData.WeatherPhenomenon);
                throw new InvalidOperationException("Usage of selected vehicle type is forbidden");
            }
        }

        /// <summary>
        /// Calculates extra fees based on weather conditions
        /// </summary>
        /// <param name="vehicleType">The vehicle type</param>
        /// <param name="weatherData">The weather data</param>
        /// <returns>The total extra fees</returns>
        private decimal CalculateExtraFees(string vehicleType, WeatherData weatherData)
        {
            decimal extraFees = 0;

            // Calculate air temperature extra fee (ATEF)
            if (vehicleType == "Bike" || vehicleType == "Scooter")
            {
                if (weatherData.AirTemperature < -10.0m)
                {
                    extraFees += 1.0m;
                    _logger.LogInformation("Adding air temperature extra fee: 1.0 € (temperature below -10°C)");
                }
                else if (weatherData.AirTemperature >= -10.0m && weatherData.AirTemperature < 0.0m)
                {
                    extraFees += 0.5m;
                    _logger.LogInformation("Adding air temperature extra fee: 0.5 € (temperature between -10°C and 0°C)");
                }
            }

            // Calculate wind speed extra fee (WSEF)
            if (vehicleType == "Bike" && weatherData.WindSpeed >= 10.0m && weatherData.WindSpeed <= 20.0m)
            {
                extraFees += 0.5m;
                _logger.LogInformation("Adding wind speed extra fee: 0.5 € (wind speed between 10 m/s and 20 m/s)");
            }

            // Calculate weather phenomenon extra fee (WPEF)
            if (vehicleType == "Bike" || vehicleType == "Scooter")
            {
                if (_snowOrSleetPhenomena.Any(p => weatherData.WeatherPhenomenon.Contains(p, StringComparison.OrdinalIgnoreCase)))
                {
                    extraFees += 1.0m;
                    _logger.LogInformation("Adding weather phenomenon extra fee: 1.0 € (snow or sleet)");
                }
                else if (_rainPhenomena.Any(p => weatherData.WeatherPhenomenon.Contains(p, StringComparison.OrdinalIgnoreCase)))
                {
                    extraFees += 0.5m;
                    _logger.LogInformation("Adding weather phenomenon extra fee: 0.5 € (rain)");
                }
            }

            return extraFees;
        }
    }
}