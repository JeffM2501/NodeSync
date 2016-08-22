using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace KeyGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(1024);
            Console.WriteLine("Private Key (For Lobby Controller)");
            Console.WriteLine(Convert.ToBase64String(rsa.ExportCspBlob(true)));
            Console.WriteLine();
            Console.WriteLine("Public Key (For Node Controller)");
            Console.WriteLine(Convert.ToBase64String(rsa.ExportCspBlob(false)));

            /*
            uint min = EncodingTools.UnixTime.GetTokenMinutes();
            byte[] t = BitConverter.GetBytes(min);
            byte[] hash = rsa.SignData(t, new SHA1CryptoServiceProvider());

            byte[] saltedData= new byte[t.Length + 32];
            new Random().NextBytes(saltedData);
            Array.Copy(t, saltedData, t.Length);
            byte[] saltedHash = rsa.SignData(saltedData, new SHA1CryptoServiceProvider());

            Console.WriteLine();
            Console.WriteLine("Time seconds is " + min.ToString());
            Console.WriteLine("Time Hash is " + Convert.ToBase64String(hash));
            Console.WriteLine();
            Console.WriteLine("SaltedTime Hash is " + Convert.ToBase64String(saltedHash));
            Console.WriteLine();

            StringBuilder code = new StringBuilder();
            if (saltedHash.Length > 3)
            {
                code.Append(saltedHash[0].ToString());
                code.Append(saltedHash[1].ToString());

                code.Append(saltedHash[saltedHash.Length - 2].ToString());
                code.Append(saltedHash[saltedHash.Length - 1].ToString());
            }
            Console.WriteLine("Generated code " + code.ToString());
            */

            Console.WriteLine();
            Console.WriteLine("Press Any key to exit");
            Console.ReadKey();
        }
    }
}
