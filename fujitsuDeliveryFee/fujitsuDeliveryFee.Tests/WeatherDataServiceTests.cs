using fujitsuDeliveryFee.Domain.Entities;
using fujitsuDeliveryFee.Infrastructure.Data;
using fujitsuDeliveryFee.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace fujitsuDeliveryFee.Tests
{
    public class WeatherDataServiceTests
    {
        private readonly Mock<ILogger<WeatherDataService>> _mockLogger;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly ApplicationDbContext _dbContext;
        private readonly WeatherDataService _service;

        public WeatherDataServiceTests()
        {
            _mockLogger = new Mock<ILogger<WeatherDataService>>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            // Set up in-memory database for testing
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _service = new WeatherDataService(_dbContext, _mockHttpClientFactory.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task FetchWeatherDataAsync_ValidResponse_ReturnsWeatherData()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(GetSampleXmlResponse())
                });

            var client = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);

            // Act
            var result = await _service.FetchWeatherDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count()); // We expect 3 stations (Tallinn, Tartu, Pärnu)
            Assert.Contains(result, w => w.City == "Tallinn");
            Assert.Contains(result, w => w.City == "Tartu");
            Assert.Contains(result, w => w.City == "Pärnu");
        }

        [Fact]
        public async Task GetLatestWeatherDataForCityAsync_ExistingCity_ReturnsLatestData()
        {
            // Arrange
            var city = "Tallinn";
            var oldData = new WeatherData
            {
                City = city,
                StationName = "Tallinn-Harku",
                AirTemperature = 5.0m,
                WindSpeed = 3.0m,
                WeatherPhenomenon = "Clear",
                Timestamp = DateTime.Now.AddHours(-2)
            };

            var newData = new WeatherData
            {
                City = city,
                StationName = "Tallinn-Harku",
                AirTemperature = 6.0m,
                WindSpeed = 4.0m,
                WeatherPhenomenon = "Light rain",
                Timestamp = DateTime.Now
            };

            await _dbContext.WeatherData.AddRangeAsync(new[] { oldData, newData });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _service.GetLatestWeatherDataForCityAsync(city);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newData.AirTemperature, result.AirTemperature);
            Assert.Equal(newData.WindSpeed, result.WindSpeed);
            Assert.Equal(newData.WeatherPhenomenon, result.WeatherPhenomenon);
        }

        [Fact]
        public async Task SaveWeatherDataAsync_ValidData_SavesToDatabase()
        {
            // Arrange
            var weatherData = new List<WeatherData>
            {
                new WeatherData
                {
                    City = "Tallinn",
                    StationName = "Tallinn-Harku",
                    AirTemperature = 5.0m,
                    WindSpeed = 3.0m,
                    WeatherPhenomenon = "Clear",
                    Timestamp = DateTime.Now
                },
                new WeatherData
                {
                    City = "Tartu",
                    StationName = "Tartu-Tõravere",
                    AirTemperature = -2.1m,
                    WindSpeed = 4.7m,
                    WeatherPhenomenon = "Light snow shower",
                    Timestamp = DateTime.Now
                }
            };

            // Act
            await _service.SaveWeatherDataAsync(weatherData);

            // Assert
            var savedData = await _dbContext.WeatherData.ToListAsync();
            Assert.Equal(2, savedData.Count);
            Assert.Contains(savedData, w => w.City == "Tallinn");
            Assert.Contains(savedData, w => w.City == "Tartu");
        }

        /// <summary>
        /// Returns a sample XML response for testing
        /// </summary>
        /// <returns>A sample XML string</returns>
        private string GetSampleXmlResponse()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<observations timestamp=""2023-04-15T12:00:00Z"">
  <station>
    <name>Tallinn-Harku</name>
    <wmocode>26038</wmocode>
    <longitude>24.602778</longitude>
    <latitude>59.398889</latitude>
    <airtemperature>5.0</airtemperature>
    <windspeed>3.0</windspeed>
    <phenomenon>Clear</phenomenon>
  </station>
  <station>
    <name>Tartu-Tõravere</name>
    <wmocode>26242</wmocode>
    <longitude>26.467222</longitude>
    <latitude>58.264722</latitude>
    <airtemperature>-2.1</airtemperature>
    <windspeed>4.7</windspeed>
    <phenomenon>Light snow shower</phenomenon>
  </station>
  <station>
    <name>Pärnu</name>
    <wmocode>41803</wmocode>
    <longitude>24.502778</longitude>
    <latitude>58.429722</latitude>
    <airtemperature>3.5</airtemperature>
    <windspeed>5.2</windspeed>
    <phenomenon>Light rain</phenomenon>
  </station>
  <station>
    <name>Kuressaare</name>
    <wmocode>26231</wmocode>
    <longitude>22.506944</longitude>
    <latitude>58.230278</latitude>
    <airtemperature>4.2</airtemperature>
    <windspeed>6.1</windspeed>
    <phenomenon>Moderate rain</phenomenon>
  </station>
</observations>";
        }
    }
}