using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;

namespace EncodingTools
{
    public static class Encryption
    {
        private static int KeySize = 256;

        public static string Encrypt(string plainText, string passPhrase, string salt)
        {
            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);

            //	Rfc2898DeriveBytes password = new Rfc2898DeriveBytes(passPhrase, nullptr);
            byte[] keyBytes = password.GetBytes(KeySize / 8);

            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Key = Convert.FromBase64String(passPhrase);
            symmetricKey.IV = Convert.FromBase64String(salt);

            return Encrypt(plainText, symmetricKey);
        }

        public static string Encrypt(string plainText, RijndaelManaged symmetricKey)
        {
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            ICryptoTransform  encryptor = symmetricKey.CreateEncryptor();
            MemoryStream  memoryStream = new MemoryStream();
            CryptoStream  cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();

            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();

            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string cryptoText, string passPhrase, string salt)
        {
            byte[] initVectorBytes = System.Text.Encoding.UTF8.GetBytes(salt);

            PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null);

            byte[] keyBytes = password.GetBytes(KeySize / 8);

            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            symmetricKey.Key = Convert.FromBase64String(passPhrase);
            symmetricKey.IV = Convert.FromBase64String(salt);

            return Decrypt(cryptoText, symmetricKey);
        }

        public static string Decrypt(string cryptoText, RijndaelManaged symmetricKey)
        {
            ICryptoTransform  decryptor = symmetricKey.CreateDecryptor();

            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(cryptoText));
            StreamReader sr = new StreamReader(new CryptoStream(ms, decryptor, CryptoStreamMode.Read));
            string ret = sr.ReadToEnd();
            sr.Close();
            ms.Close();

            return ret;
        }

        public static RijndaelManaged BuildCrypto(string tokenSalt)
        {
            RijndaelManaged crypto = new RijndaelManaged();
            crypto.BlockSize = 256;
            string[] parts = tokenSalt.Split(":".ToCharArray(), 2);

            if (parts.Length == 2)
            {
                string key = parts[0];
                string iv = parts[1];

                crypto.Key = Convert.FromBase64String(key);
                crypto.IV = Convert.FromBase64String(iv);
                return crypto;
            }
            return null;
        }
    }
}
