using System;
using System.Text;
using System.Security.Cryptography;

namespace EncodingTools
{
    public static class Tokens
    {
        private static object HashProvider = new SHA512CryptoServiceProvider();

        public static string GenerateTimeToken(RSACryptoServiceProvider rsa, string apiKey)
        {
            uint min = UnixTime.GetTokenMinutes();
            byte[] t = BitConverter.GetBytes(min);

            byte[] keyBytes = Encoding.UTF8.GetBytes(apiKey);

            byte[] saltedData = new byte[t.Length + keyBytes.Length];
            Array.Copy(keyBytes,0, saltedData, t.Length, keyBytes.Length);
            Array.Copy(t, saltedData, t.Length);

            return Convert.ToBase64String(rsa.SignData(saltedData, HashProvider));
        }

        public static bool ValidateTimeToken(RSACryptoServiceProvider rsa, string apiKey, string hash, uint time)
        {
            byte[] t = BitConverter.GetBytes(time);

            byte[] keyBytes = Encoding.UTF8.GetBytes(apiKey);

            byte[] saltedData = new byte[t.Length + keyBytes.Length];
            Array.Copy(keyBytes, 0, saltedData, t.Length, keyBytes.Length);
            Array.Copy(t, saltedData, t.Length);

            return rsa.VerifyData(saltedData, HashProvider, Convert.FromBase64String(hash));
        }

        public static bool ValidateCurrentTimeToken(RSACryptoServiceProvider rsa, string apiKey, string hash, int range)
        {
            uint now = UnixTime.GetTokenMinutes();
            if (ValidateTimeToken(rsa, apiKey, hash, now))
                return true;

            for(int i = 1; i <= range; i++)
            {
                if (ValidateTimeToken(rsa, apiKey, hash, (uint)(now + i)))
                    return true;

                if (ValidateTimeToken(rsa, apiKey, hash, (uint)(now - i)))
                    return true;
            }

            return false;
        }
    }
}
