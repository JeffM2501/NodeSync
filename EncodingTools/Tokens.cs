using System;
using System.Text;
using System.Security.Cryptography;

namespace EncodingTools
{
    public static class Tokens
    {
        public static string GenerateTimeToken(RijndaelManaged crypto)
        {
            uint min = UnixTime.GetTokenMinutes();

            return Encryption.Encrypt(min.ToString(), crypto);
        }

        public static bool ValidateTimeToken(RijndaelManaged crypto, string value, uint now)
        {
            string timeString = Encryption.Decrypt(value, crypto);
            uint time = uint.MinValue;
            if (!uint.TryParse(timeString, out time))
                return false;

            return time == now;
        }

        public static bool ValidateCurrentTimeToken(RijndaelManaged crypto, string value, int range)
        {
            uint now = UnixTime.GetTokenMinutes();
            if (ValidateTimeToken(crypto, value, now))
                return true;

            for(int i = 1; i <= range; i++)
            {
                if (ValidateTimeToken(crypto, value,(uint)(now + i)))
                    return true;

                if (ValidateTimeToken(crypto, value,(uint)(now - i)))
                    return true;
            }

            return false;
        }
    }
}
