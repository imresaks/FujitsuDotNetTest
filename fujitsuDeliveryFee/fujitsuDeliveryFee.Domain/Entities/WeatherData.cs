using System;

namespace fujitsuDeliveryFee.Domain.Entities
{
    /// <summary>
    /// Represents weather data from a specific station at a specific time
    /// </summary>
    public class WeatherData
    {
        /// <summary>
        /// Unique identifier for the weather data record
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the weather station
        /// </summary>
        public string StationName { get; set; } = string.Empty;

        /// <summary>
        /// WMO code of the station
        /// </summary>
        public string WmoCode { get; set; } = string.Empty;

        /// <summary>
        /// Air temperature in degrees Celsius
        /// </summary>
        public decimal AirTemperature { get; set; }

        /// <summary>
        /// Wind speed in meters per second
        /// </summary>
        public decimal WindSpeed { get; set; }

        /// <summary>
        /// Weather phenomenon description (e.g., "Light snow", "Rain")
        /// </summary>
        public string WeatherPhenomenon { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of the weather observation
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// City associated with this weather station
        /// </summary>
        public string City { get; set; } = string.Empty;
    }
}