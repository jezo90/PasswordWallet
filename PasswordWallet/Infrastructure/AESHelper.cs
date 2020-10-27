using PasswordWallet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PasswordWallet.Infrastructure
{
    public class AESHelper
    {
        public static byte[] EncryptString(string toEncrypt, string salt)
        {
            var key = GetKey(salt);

            using (var aes = Aes.Create())
            using (var encryptor = aes.CreateEncryptor(key, key))
            {
                var plainText = Encoding.UTF8.GetBytes(toEncrypt);
                return encryptor.TransformFinalBlock(plainText, 0, plainText.Length);
            }
        }

        public static string DecryptToString(byte[] encryptedData, string salt)
        {
            var key = GetKey(salt);

            using (var aes = Aes.Create())
            using (var encryptor = aes.CreateDecryptor(key, key))
            {
                var decryptedBytes = encryptor
                    .TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }

        private static byte[] GetKey(string password)
        {
            var keyBytes = Encoding.UTF8.GetBytes(password);
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(keyBytes);
            }
        }

        public static List<Passwd> rehashPasswds(List<Passwd> passwordsToChange, string currentMaster, string newMaster)
        {
            foreach(Passwd passwd in passwordsToChange)
            {
                passwd.Password = DecryptToString(Convert.FromBase64String(passwd.Password), currentMaster); // decrypt with current password
                passwd.Password = Convert.ToBase64String(EncryptString(passwd.Password, newMaster)); // encrypt with new password
            }
            return passwordsToChange;
        }
    }
}
