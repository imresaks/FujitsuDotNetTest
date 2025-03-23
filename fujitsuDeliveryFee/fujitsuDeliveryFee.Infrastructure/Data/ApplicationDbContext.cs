using fujitsuDeliveryFee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace fujitsuDeliveryFee.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<WeatherData> WeatherData { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<RegionalBaseFee> RegionalBaseFees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure WeatherData entity
            modelBuilder.Entity<WeatherData>()
                .HasIndex(w => w.Timestamp);

            modelBuilder.Entity<WeatherData>()
                .HasIndex(w => w.City);

            // Configure RegionalBaseFee entity
            modelBuilder.Entity<RegionalBaseFee>()
                .HasOne(r => r.City)
                .WithMany()
                .HasForeignKey(r => r.CityId);

            modelBuilder.Entity<RegionalBaseFee>()
                .HasOne(r => r.VehicleType)
                .WithMany()
                .HasForeignKey(r => r.VehicleTypeId);

            // Seed data for cities
            modelBuilder.Entity<City>().HasData(
                new City { Id = 1, Name = "Tallinn", StationName = "Tallinn-Harku" },
                new City { Id = 2, Name = "Tartu", StationName = "Tartu-Tõravere" },
                new City { Id = 3, Name = "Pärnu", StationName = "Pärnu" }
            );

            // Seed data for vehicle types
            modelBuilder.Entity<VehicleType>().HasData(
                new VehicleType { Id = 1, Name = "Car" },
                new VehicleType { Id = 2, Name = "Scooter" },
                new VehicleType { Id = 3, Name = "Bike" }
            );

            // Seed data for regional base fees
            modelBuilder.Entity<RegionalBaseFee>().HasData(
                // Tallinn
                new RegionalBaseFee { Id = 1, CityId = 1, VehicleTypeId = 1, Fee = 4.0m },   // Car
                new RegionalBaseFee { Id = 2, CityId = 1, VehicleTypeId = 2, Fee = 3.5m },   // Scooter
                new RegionalBaseFee { Id = 3, CityId = 1, VehicleTypeId = 3, Fee = 3.0m },   // Bike
                
                // Tartu
                new RegionalBaseFee { Id = 4, CityId = 2, VehicleTypeId = 1, Fee = 3.5m },   // Car
                new RegionalBaseFee { Id = 5, CityId = 2, VehicleTypeId = 2, Fee = 3.0m },   // Scooter
                new RegionalBaseFee { Id = 6, CityId = 2, VehicleTypeId = 3, Fee = 2.5m },   // Bike
                
                // Pärnu
                new RegionalBaseFee { Id = 7, CityId = 3, VehicleTypeId = 1, Fee = 3.0m },   // Car
                new RegionalBaseFee { Id = 8, CityId = 3, VehicleTypeId = 2, Fee = 2.5m },   // Scooter
                new RegionalBaseFee { Id = 9, CityId = 3, VehicleTypeId = 3, Fee = 2.0m }    // Bike
            );
        }
    }
}
