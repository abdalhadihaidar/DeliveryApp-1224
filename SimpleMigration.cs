using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        Console.WriteLine("AppUser Data Migration Tool");
        Console.WriteLine("===========================");
        
        string connectionString = "Server=localhost;Database=DeliveryApp;Trusted_Connection=true;TrustServerCertificate=true;";
        
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to database successfully.");
                
                // Check current state
                CheckCurrentState(connection);
                
                // Run migration
                RunMigration(connection);
                
                // Verify migration
                VerifyMigration(connection);
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
    
    static void CheckCurrentState(SqlConnection connection)
    {
        Console.WriteLine("\n--- Checking Current State ---");
        
        try
        {
            // Check AppUsers count
            var appUsersCount = ExecuteScalar(connection, "SELECT COUNT(*) FROM AppUsers");
            Console.WriteLine($"AppUsers table: {appUsersCount} records");
            
            // Check AbpUsers count
            var abpUsersCount = ExecuteScalar(connection, "SELECT COUNT(*) FROM AbpUsers");
            Console.WriteLine($"AbpUsers table: {abpUsersCount} records");
            
            // Check for AppUsers not in AbpUsers
            var missingCount = ExecuteScalar(connection, 
                "SELECT COUNT(*) FROM AppUsers au LEFT JOIN AbpUsers abu ON au.Id = abu.Id WHERE abu.Id IS NULL");
            Console.WriteLine($"AppUsers not in AbpUsers: {missingCount} records");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking state: {ex.Message}");
        }
    }
    
    static void RunMigration(SqlConnection connection)
    {
        Console.WriteLine("\n--- Running Migration ---");
        
        try
        {
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
            
            var insertedRows = ExecuteNonQuery(connection, insertSql);
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
            
            var updatedRows = ExecuteNonQuery(connection, updateSql);
            Console.WriteLine($"Updated {updatedRows} existing users in AbpUsers");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during migration: {ex.Message}");
        }
    }
    
    static void VerifyMigration(SqlConnection connection)
    {
        Console.WriteLine("\n--- Verifying Migration ---");
        
        try
        {
            // Check final counts
            var appUsersCount = ExecuteScalar(connection, "SELECT COUNT(*) FROM AppUsers");
            var abpUsersCount = ExecuteScalar(connection, "SELECT COUNT(*) FROM AbpUsers");
            
            Console.WriteLine($"Final AppUsers count: {appUsersCount}");
            Console.WriteLine($"Final AbpUsers count: {abpUsersCount}");
            
            // Check for any remaining missing users
            var missingCount = ExecuteScalar(connection, 
                "SELECT COUNT(*) FROM AppUsers au LEFT JOIN AbpUsers abu ON au.Id = abu.Id WHERE abu.Id IS NULL");
            
            if (missingCount == 0)
            {
                Console.WriteLine("✅ All AppUsers are now in AbpUsers table");
            }
            else
            {
                Console.WriteLine($"❌ {missingCount} AppUsers are still missing from AbpUsers table");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during verification: {ex.Message}");
        }
    }
    
    static object ExecuteScalar(SqlConnection connection, string sql)
    {
        using (var command = new SqlCommand(sql, connection))
        {
            return command.ExecuteScalar();
        }
    }
    
    static int ExecuteNonQuery(SqlConnection connection, string sql)
    {
        using (var command = new SqlCommand(sql, connection))
        {
            return command.ExecuteNonQuery();
        }
    }
}
