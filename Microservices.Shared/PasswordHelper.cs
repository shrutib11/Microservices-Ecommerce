using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Microservices.Shared
{
    public class PasswordHelper
    {
        public static string HashPassword(string password)
        {

            byte[] salt = RandomNumberGenerator.GetBytes(16);

            var hash = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hashBytes = hash.GetBytes(32);


            byte[] hashWithSaltBytes = new byte[48];
            Array.Copy(salt, 0, hashWithSaltBytes, 0, 16);
            Array.Copy(hashBytes, 0, hashWithSaltBytes, 16, 32);

            return Convert.ToBase64String(hashWithSaltBytes);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            byte[] hashWithSaltBytes = Convert.FromBase64String(storedHash);
            byte[] salt = new byte[16];
            Array.Copy(hashWithSaltBytes, 0, salt, 0, 16);

            var hash = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hashBytes = hash.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashWithSaltBytes[i + 16] != hashBytes[i])
                    return false;
            }
            return true;
        }
    }
}