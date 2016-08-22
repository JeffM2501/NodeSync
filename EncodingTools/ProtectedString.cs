using System;
using System.Text;
using System.Security.Cryptography;

namespace EncodingTools
{
    public static class ProtectedString
    {
        public static string Protect(this string str)
        {
            byte[] o = ProtectedData.Protect(Encoding.UTF8.GetBytes(str), null, DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(o);
        }

        public static string Unprotect(this string str)
        {
            byte[] o = ProtectedData.Unprotect(Convert.FromBase64String(str), null, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(o);
        }
    }
}
