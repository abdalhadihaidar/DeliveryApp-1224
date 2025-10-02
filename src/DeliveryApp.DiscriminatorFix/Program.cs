using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DeliveryApp.EntityFrameworkCore;
using DeliveryApp.EntityFrameworkCore.Data;

namespace DeliveryApp.DiscriminatorFix
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Discriminator Fix Tool");
            Console.WriteLine("=====================");
            Console.WriteLine();

            try
            {
                // Create host builder
                var host = CreateHostBuilder(args).Build();

                // Get the discriminator fix service
                var discriminatorService = host.Services.GetRequiredService<DiscriminatorDataMigrationService>();

                Console.WriteLine("Starting discriminator fix...");
                Console.WriteLine();

                // Run the fix
                await discriminatorService.FixDiscriminatorValuesAsync();

                Console.WriteLine();
                Console.WriteLine("Discriminator fix completed successfully!");
                Console.WriteLine("You can now run the application without discriminator errors.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("Please check your database connection and try again.");
                Console.WriteLine("Make sure the database is accessible and the connection string is correct.");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Add Entity Framework
                    services.AddDbContext<DeliveryAppDbContext>(options =>
                    {
                        // You may need to adjust the connection string here
                        options.UseSqlServer("Server=localhost;Database=DeliveryAppDb;Integrated Security=true;TrustServerCertificate=true;");
                    });

                    // Add the discriminator fix service
                    services.AddTransient<DiscriminatorDataMigrationService>();
                });
    }
}
