using fujitsuDeliveryFee.Application.Services;
using fujitsuDeliveryFee.Domain.Entities;
using fujitsuDeliveryFee.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace fujitsuDeliveryFee.Tests
{
    public class DeliveryFeeCalculationServiceTests
    {
        private readonly Mock<IWeatherDataService> _mockWeatherDataService;
        private readonly Mock<ILogger<DeliveryFeeCalculationService>> _mockLogger;
        private readonly DeliveryFeeCalculationService _service;

        public DeliveryFeeCalculationServiceTests()
        {
            _mockWeatherDataService = new Mock<IWeatherDataService>();
            _mockLogger = new Mock<ILogger<DeliveryFeeCalculationService>>();
            _service = new DeliveryFeeCalculationService(_mockWeatherDataService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task CalculateDeliveryFeeAsync_ValidInputs_ReturnsCorrectFee()
        {
            // Arrange
            var city = "Tartu";
            var vehicleType = "Bike";
            var weatherData = new WeatherData
            {
                City = city,
                AirTemperature = -2.1m,
                WindSpeed = 4.7m,
                WeatherPhenomenon = "Light snow shower",
                Timestamp = DateTime.Now
            };

            _mockWeatherDataService.Setup(s => s.GetLatestWeatherDataForCityAsync(city))
                .ReturnsAsync(weatherData);

            // Act
            var result = await _service.CalculateDeliveryFeeAsync(city, vehicleType);

            // Assert
            // Base fee (2.5) + Temperature fee (0.5) + Wind fee (0) + Weather phenomenon fee (1.0) = 4.0
            Assert.Equal(4.0m, result);
        }

        [Fact]
        public async Task CalculateDeliveryFeeAsync_ForbiddenWindSpeed_ThrowsException()
        {
            // Arrange
            var city = "Tallinn";
            var vehicleType = "Bike";
            var weatherData = new WeatherData
            {
                City = city,
                AirTemperature = 5.0m,
                WindSpeed = 21.0m, // Above 20 m/s is forbidden for bikes
                WeatherPhenomenon = "Clear",
                Timestamp = DateTime.Now
            };

            _mockWeatherDataService.Setup(s => s.GetLatestWeatherDataForCityAsync(city))
                .ReturnsAsync(weatherData);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CalculateDeliveryFeeAsync(city, vehicleType));
            
            Assert.Contains("forbidden", exception.Message);
        }

        [Fact]
        public async Task CalculateDeliveryFeeAsync_ForbiddenWeatherPhenomenon_ThrowsException()
        {
            // Arrange
            var city = "Pärnu";
            var vehicleType = "Scooter";
            var weatherData = new WeatherData
            {
                City = city,
                AirTemperature = 10.0m,
                WindSpeed = 5.0m,
                WeatherPhenomenon = "Thunder", // Thunder is forbidden for scooters
                Timestamp = DateTime.Now
            };

            _mockWeatherDataService.Setup(s => s.GetLatestWeatherDataForCityAsync(city))
                .ReturnsAsync(weatherData);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CalculateDeliveryFeeAsync(city, vehicleType));
            
            Assert.Contains("forbidden", exception.Message);
        }

        [Fact]
        public async Task CalculateDeliveryFeeAsync_InvalidCity_ThrowsArgumentException()
        {
            // Arrange
            var city = "InvalidCity";
            var vehicleType = "Car";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CalculateDeliveryFeeAsync(city, vehicleType));
            
            Assert.Contains("Invalid city", exception.Message);
        }

        [Fact]
        public async Task CalculateDeliveryFeeAsync_InvalidVehicleType_ThrowsArgumentException()
        {
            // Arrange
            var city = "Tallinn";
            var vehicleType = "InvalidVehicle";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => _service.CalculateDeliveryFeeAsync(city, vehicleType));
            
            Assert.Contains("Invalid vehicle type", exception.Message);
        }
    }
}