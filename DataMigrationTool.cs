using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace DeliveryApp.DataMigration
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("AppUser Data Migration Tool");
            Console.WriteLine("===========================");
            
            // Connection string - adjust as needed
            string connectionString = "Server=localhost;Database=DeliveryApp;Trusted_Connection=true;TrustServerCertificate=true;";
            
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    Console.WriteLine("Connected to database successfully.");
                    
                    // Check current state
                    await CheckCurrentState(connection);
                    
                    // Run migration
                    await RunMigration(connection);
                    
                    // Verify migration
                    await VerifyMigration(connection);
                }
                
                Console.WriteLine("\nMigration completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Migration failed!");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        static async Task CheckCurrentState(SqlConnection connection)
        {
            Console.WriteLine("\n--- Checking Current State ---");
            
            // Check AppUsers count
            var appUsersCount = await ExecuteScalarAsync(connection, "SELECT COUNT(*) FROM AppUsers");
            Console.WriteLine($"AppUsers table: {appUsersCount} records");
            
            // Check AbpUsers count
            var abpUsersCount = await ExecuteScalarAsync(connection, "SELECT COUNT(*) FROM AbpUsers");
            Console.WriteLine($"AbpUsers table: {abpUsersCount} records");
            
            // Check for AppUsers not in AbpUsers
            var missingCount = await ExecuteScalarAsync(connection, 
                "SELECT COUNT(*) FROM AppUsers au LEFT JOIN AbpUsers abu ON au.Id = abu.Id WHERE abu.Id IS NULL");
            Console.WriteLine($"AppUsers not in AbpUsers: {missingCount} records");
        }
        
        static async Task RunMigration(SqlConnection connection)
        {
            Console.WriteLine("\n--- Running Migration ---");
            
            // Insert missing AppUsers into AbpUsers
            var insertSql = @"
                INSERT INTO AbpUsers (
                    Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, 
                    PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, 
                    TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount, Name, Surname, 
                    IsActive, IsDeleted, CreationTime, LastModificationTime, TenantId,
                    ProfileImageUrl, ReviewStatus, ReviewReason
                )
                SELECT 
                    au.Id, au.UserName, UPPER(au.UserName), au.Email, UPPER(au.Email), 0,
                    '', NEWID(), NEWID(), '', 0, 0, NULL, 1, 0, au.Name, au.Surname,
                    1, 0, GETUTCDATE(), GETUTCDATE(), au.TenantId,
                    au.ProfileImageUrl, au.ReviewStatus, au.ReviewReason
                FROM AppUsers au
                LEFT JOIN AbpUsers abu ON au.Id = abu.Id
                WHERE abu.Id IS NULL";
            
            var insertedRows = await ExecuteNonQueryAsync(connection, insertSql);
            Console.WriteLine($"Inserted {insertedRows} new users into AbpUsers");
            
            // Update existing AbpUsers with AppUser data
            var updateSql = @"
                UPDATE abu
                SET 
                    ProfileImageUrl = ISNULL(au.ProfileImageUrl, abu.ProfileImageUrl),
                    ReviewStatus = ISNULL(au.ReviewStatus, abu.ReviewStatus),
                    ReviewReason = ISNULL(au.ReviewReason, abu.ReviewReason)
                FROM AbpUsers abu
                INNER JOIN AppUsers au ON abu.Id = au.Id";
            
            var updatedRows = await ExecuteNonQueryAsync(connection, updateSql);
            Console.WriteLine($"Updated {updatedRows} existing users in AbpUsers");
        }
        
        static async Task VerifyMigration(SqlConnection connection)
        {
            Console.WriteLine("\n--- Verifying Migration ---");
            
            // Check final counts
            var appUsersCount = await ExecuteScalarAsync(connection, "SELECT COUNT(*) FROM AppUsers");
            var abpUsersCount = await ExecuteScalarAsync(connection, "SELECT COUNT(*) FROM AbpUsers");
            
            Console.WriteLine($"Final AppUsers count: {appUsersCount}");
            Console.WriteLine($"Final AbpUsers count: {abpUsersCount}");
            
            // Check for any remaining missing users
            var missingCount = await ExecuteScalarAsync(connection, 
                "SELECT COUNT(*) FROM AppUsers au LEFT JOIN AbpUsers abu ON au.Id = abu.Id WHERE abu.Id IS NULL");
            
            if (missingCount == 0)
            {
                Console.WriteLine("✅ All AppUsers are now in AbpUsers table");
            }
            else
            {
                Console.WriteLine($"❌ {missingCount} AppUsers are still missing from AbpUsers table");
            }
            
            // Show sample of migrated data
            Console.WriteLine("\nSample of migrated data:");
            var sampleSql = @"
                SELECT TOP 3 
                    abu.Id, abu.UserName, abu.Email, abu.Name, 
                    abu.ProfileImageUrl, abu.ReviewStatus
                FROM AbpUsers abu
                INNER JOIN AppUsers au ON abu.Id = au.Id
                ORDER BY abu.CreationTime DESC";
            
            using (var command = new SqlCommand(sampleSql, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    Console.WriteLine($"  ID: {reader["Id"]}, Name: {reader["Name"]}, Email: {reader["Email"]}, ReviewStatus: {reader["ReviewStatus"]}");
                }
            }
        }
        
        static async Task<object> ExecuteScalarAsync(SqlConnection connection, string sql)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                return await command.ExecuteScalarAsync();
            }
        }
        
        static async Task<int> ExecuteNonQueryAsync(SqlConnection connection, string sql)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                return await command.ExecuteNonQueryAsync();
            }
        }
    }
}
