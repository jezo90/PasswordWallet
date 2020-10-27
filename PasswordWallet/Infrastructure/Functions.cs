using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using PasswordWallet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordWallet.Infrastructure
{
    public class Functions
    {
        public static User getUser(IMemoryCache cache)
        {
            User Usr = cache.Get(CacheNames.user) as User;
            if (Usr == null)
            {
                Usr = new User();
            }
            return Usr;
        }

        public static string getLogged(IMemoryCache cache)
        {
            string logged1 = "0";
            var logged = cache.Get(CacheNames.logged);
            if (logged != null)
            {
                logged1 = "1";
            }
            return logged1;
        }

        public static string SHA512(string plainText, byte[] saltBytes)
        {
            // Convert plain text into a byte array.+
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            // Allocate array, which will hold plain text and salt.
            byte[] plainTextWithSaltBytes =
                    new byte[plainTextBytes.Length + saltBytes.Length];

            // Copy plain text bytes into resulting array.
            for (int i = 0; i < plainTextBytes.Length; i++)
                plainTextWithSaltBytes[i] = plainTextBytes[i];

            // Append salt bytes to the resulting array.
            for (int i = 0; i < saltBytes.Length; i++)
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];


            HashAlgorithm hash;
            hash = new SHA512Managed();


            // Compute hash value of our plain text with appended salt.
            byte[] hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            // Create array which will hold hash and original salt bytes.
            byte[] hashWithSaltBytes = new byte[hashBytes.Length +
                                                saltBytes.Length];

            // Copy hash bytes into resulting array.
            for (int i = 0; i < hashBytes.Length; i++)
                hashWithSaltBytes[i] = hashBytes[i];

            // Append salt bytes to the result.
            for (int i = 0; i < saltBytes.Length; i++)
                hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];

            // Convert result into a base64-encoded string.
            string hashValue = Convert.ToBase64String(hashWithSaltBytes);

            // Return the result.
            return hashValue;
        }

        public static string GenerateHMAC(string message, HMAC hmac)
        {
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(message)));
        }

        public static byte[] GenerateSalt()
        {
            byte[] saltBytes;

            // Define min and max salt sizes.
            int minSaltSize = 8;
            int maxSaltSize = 8;

            // Generate a random number for the size of the salt.
            Random random = new Random();
            int saltSize = random.Next(minSaltSize, maxSaltSize);

            // Allocate a byte array, which will hold the salt.
            saltBytes = new byte[saltSize];

            // Initialize a random number generator.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            // Fill the salt with cryptographically strong byte values.
            rng.GetNonZeroBytes(saltBytes);
            return saltBytes;
        }

        public static byte[] StringToBytes(string zm1)
        {
            return Encoding.UTF8.GetBytes(zm1);
        }

        public static string BytesToString(byte[] zm1)
        {
            return Convert.ToBase64String(zm1);
        }
    }
}
