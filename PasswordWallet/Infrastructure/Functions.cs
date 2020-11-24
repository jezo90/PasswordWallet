using Microsoft.Extensions.Caching.Memory;
using PasswordWallet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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

        public static User UnblockUser(User user)
        {
            if (user.AccountBlockDate < DateTime.Now)
            {
                user.IsAccountBlocked = false;
            }
            return user;
        }

        public static bool IsUserBlocked(User user)
        {
            return user.IsAccountBlocked;
        }

        public static bool IsIpBlocked(AddressIP ip)
        {
            return (ip.IpBlockDate < DateTime.Now);
        }

        public static AddressIP UnblockIP(AddressIP ip)
        {
            ip.Correct = 0;
            ip.Incorrect = 0;
            ip.IpBlockDate = DateTime.Now.AddMinutes(-1);
            return ip;
        }

        public static bool Login(User user, string password)
        {
            user = UnblockUser(user);

            if (!IsUserBlocked(user))
            {
                if (user.isPasswordKeptAsHash == "SHA512")
                {
                    var salt = Functions.StringToBytes(user.Salt);
                    var pepper = Functions.StringToBytes(CacheNames.pepper);
                    var passWithSalt = Functions.SHA512(password, salt);
                    var passWithSaltAndPepper = Functions.SHA512(passWithSalt, pepper);

                    if (user.Password == passWithSaltAndPepper)
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    var salt = Functions.StringToBytes(user.Salt);
                    HMAC hmac = new HMACSHA256(salt);
                    if (user.Password == Functions.GenerateHMAC(password, hmac))
                    {
                        return true;
                    }
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool isNickAvailable(List<User> usersList, string nick)
        {
            if (usersList.Where(a => a.Nickname == nick).FirstOrDefault<User>() == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static User createUser(User registerUser)
        {
            User newUser = registerUser;
            var salt = Functions.GenerateSalt();    // generate salt
            var salt1 = Functions.StringToBytes(Functions.BytesToString(salt));
            newUser.Salt = Functions.BytesToString(salt);  // set user salt
            if (registerUser.isPasswordKeptAsHash == "SHA512")
            {
                var pepper = Functions.StringToBytes(CacheNames.pepper);    // get pepper 
                var passWithSalt = Functions.SHA512(registerUser.Password, salt1);  // hash wit salt 
                var passWithSaltAndPepper = Functions.SHA512(passWithSalt, pepper); // hash with pepper
                newUser.Password = passWithSaltAndPepper;  // set hashed with sha user password
            }
            else if (registerUser.isPasswordKeptAsHash == "HMAC")
            {
                HMAC hmac = new HMACSHA256(salt1);  //  hash hmac
                newUser.Password = Functions.GenerateHMAC(registerUser.Password, hmac); // set hased with hmac user password 
            }
            return newUser;
        }

        public static User ChangePasswordSHA(string newPassword, User userToChange)
        {
            var newsalt = Functions.GenerateSalt();
            var newsalt1 = Functions.StringToBytes(Functions.BytesToString(newsalt));
            var newPassWithSalt = Functions.SHA512(newPassword, newsalt1);
            var newPassWithSaltAndPepper = Functions.SHA512(newPassWithSalt, Functions.StringToBytes(CacheNames.pepper));

            userToChange.Salt = Functions.BytesToString(newsalt);
            userToChange.Password = newPassWithSaltAndPepper;

            return userToChange;
        }
        public static User ChangePasswordHMAC(string newPassword, User userToChange)
        {
            var newsalt = Functions.GenerateSalt();
            var newsalt1 = Functions.StringToBytes(Functions.BytesToString(newsalt));
            HMAC newhmac = new HMACSHA256(newsalt1);
            string passwordToChange = Functions.GenerateHMAC(newPassword, newhmac);

            userToChange.Salt = Functions.BytesToString(newsalt);
            userToChange.Password = passwordToChange;

            return userToChange;
        }
    }
}
