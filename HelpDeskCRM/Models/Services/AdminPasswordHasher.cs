using System;
using System.Security.Cryptography;

namespace HelpDeskCRM.Services
{
    public static class AdminPasswordHasher
    {
        // =====================================================
        // HASH PASSWORD
        // =====================================================

        public static string HashPassword(string password)
        {
            // Generate random salt
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            // Create PBKDF2 hash
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100000,
                HashAlgorithmName.SHA256
            );

            byte[] hash = pbkdf2.GetBytes(32);

            // Store salt + hash
            return Convert.ToBase64String(salt)
                   + "."
                   + Convert.ToBase64String(hash);
        }


        // =====================================================
        // VERIFY PASSWORD
        // =====================================================

        public static bool VerifyPassword(
            string password,
            string storedHash)
        {
            try
            {
                // Split stored value
                string[] parts =
                    storedHash.Split('.');

                if (parts.Length != 2)
                {
                    return false;
                }


                // Get original salt
                byte[] salt =
                    Convert.FromBase64String(parts[0]);


                // Get original password hash
                byte[] expectedHash =
                    Convert.FromBase64String(parts[1]);


                // Hash entered password using same salt
                using var pbkdf2 =
                    new Rfc2898DeriveBytes(
                        password,
                        salt,
                        100000,
                        HashAlgorithmName.SHA256
                    );


                byte[] actualHash =
                    pbkdf2.GetBytes(32);


                // Compare hashes safely
                return CryptographicOperations.FixedTimeEquals(
                    actualHash,
                    expectedHash
                );
            }
            catch
            {
                return false;
            }
        }
    }
}