using System;
using System.Security.Cryptography;
using System.Text;
using Volo.Abp.DependencyInjection;

namespace DeliveryApp.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        string GenerateSecureToken(int length = 32);
    }

    public class EncryptionService : IEncryptionService, ITransientDependency
    {
        private readonly string _encryptionKey;

        public EncryptionService()
        {
            // In production, this should come from configuration/environment variables
            _encryptionKey = "YourSecretEncryptionKey123!@#$%^&*()";
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16]; // Use a proper IV in production

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            
            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16]; // Use a proper IV in production

                using var decryptor = aes.CreateDecryptor();
                var encryptedBytes = Convert.FromBase64String(cipherText);
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty");

            // Generate a random salt
            var salt = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password with the salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            // Combine salt and hash
            var hashBytes = new byte[64];
            Array.Copy(salt, 0, hashBytes, 0, 32);
            Array.Copy(hash, 0, hashBytes, 32, 32);

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            try
            {
                var hashBytes = Convert.FromBase64String(hash);
                
                // Extract salt
                var salt = new byte[32];
                Array.Copy(hashBytes, 0, salt, 0, 32);

                // Hash the provided password with the extracted salt
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                var testHash = pbkdf2.GetBytes(32);

                // Compare hashes
                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + 32] != testHash[i])
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GenerateSecureToken(int length = 32)
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
}

