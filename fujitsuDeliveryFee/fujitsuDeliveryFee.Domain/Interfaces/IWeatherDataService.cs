using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fujitsuDeliveryFee.Domain.Entities;

namespace fujitsuDeliveryFee.Domain.Interfaces
{
    /// <summary>
    /// Interface for weather data service operations
    /// </summary>
    public interface IWeatherDataService
    {
        /// <summary>
        /// Fetches the latest weather data from the Estonian Environment Agency
        /// </summary>
        /// <returns>A collection of weather data objects</returns>
        Task<IEnumerable<WeatherData>> FetchWeatherDataAsync();

        /// <summary>
        /// Gets the latest weather data for a specific city
        /// </summary>
        /// <param name="city">The name of the city</param>
        /// <returns>The latest weather data for the specified city</returns>
        Task<WeatherData?> GetLatestWeatherDataForCityAsync(string city);

        /// <summary>
        /// Saves weather data to the database
        /// </summary>
        /// <param name="weatherData">The weather data to save</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task SaveWeatherDataAsync(IEnumerable<WeatherData> weatherData);
    }
}