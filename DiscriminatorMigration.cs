using System;
using System.Data.SqlClient;
using System.IO;

class DiscriminatorMigration
{
    static void Main()
    {
        Console.WriteLine("Starting discriminator migration...");
        
        string connectionString = "Server=localhost;Database=DeliveryApp;Integrated Security=true;TrustServerCertificate=true;";
        
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to database successfully.");
                
                // Step 1: Add discriminator column if it doesn't exist
                Console.WriteLine("Adding discriminator column...");
                string addColumnSql = @"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'AbpUsers' AND COLUMN_NAME = 'Discriminator')
                    BEGIN
                        ALTER TABLE AbpUsers ADD Discriminator NVARCHAR(50) NOT NULL DEFAULT 'IdentityUser';
                        PRINT 'Added Discriminator column to AbpUsers table.';
                    END
                    ELSE
                    BEGIN
                        PRINT 'Discriminator column already exists in AbpUsers table.';
                    END";
                
                using (var command = new SqlCommand(addColumnSql, connection))
                {
                    command.ExecuteNonQuery();
                    Console.WriteLine("Discriminator column check completed.");
                }
                
                // Step 2: Update existing records
                Console.WriteLine("Updating discriminator values...");
                string updateSql = @"
                    -- Update users that have corresponding AppUser records to be 'AppUser'
                    UPDATE abu
                    SET Discriminator = 'AppUser'
                    FROM AbpUsers abu
                    INNER JOIN AppUsers au ON abu.Id = au.Id;
                    
                    -- Update remaining users to be 'IdentityUser'
                    UPDATE abu
                    SET Discriminator = 'IdentityUser'
                    FROM AbpUsers abu
                    LEFT JOIN AppUsers au ON abu.Id = au.Id
                    WHERE au.Id IS NULL;";
                
                using (var command = new SqlCommand(updateSql, connection))
                {
                    int rowsAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"Updated {rowsAffected} records with discriminator values.");
                }
                
                // Step 3: Verify the results
                Console.WriteLine("Verifying discriminator values...");
                string verifySql = @"
                    SELECT 'Discriminator distribution:' as Info, Discriminator, COUNT(*) as Count
                    FROM AbpUsers
                    GROUP BY Discriminator;";
                
                using (var command = new SqlCommand(verifySql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Info"]}: {reader["Discriminator"]} = {reader["Count"]}");
                    }
                }
                
                Console.WriteLine("Discriminator migration completed successfully!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}


