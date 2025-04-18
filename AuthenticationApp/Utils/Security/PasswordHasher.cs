using System.Security.Cryptography;

namespace AuthenticationApp.Utils.Security
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            using var rfc2898 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            byte[] hash = rfc2898.GetBytes(KeySize);

            string base64Salt = Convert.ToBase64String(salt);
            string base64Hash = Convert.ToBase64String(hash);

            return $"{Iterations}.{base64Salt}.{base64Hash}";
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            string[] parts = hashedPassword.Split('.',3);
            if (parts.Length != 3)
            {
                return false;
            }

            int iterations = int.Parse(parts[0]);

            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] hash = Convert.FromBase64String(parts[2]);

            using var rfc2898 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] computedHash = rfc2898.GetBytes(hash.Length);
            return CryptographicOperations.FixedTimeEquals(hash, computedHash);
        }
    }
}
