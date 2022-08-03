using System.Text;
using System.Security.Cryptography;

namespace HashAxe.Crypto
{
    class Hash
    {
        public static String sha256(string data)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] rawData = Encoding.UTF8.GetBytes(data);
            byte[] hashed = sha256.ComputeHash(rawData);

            StringBuilder stringbuilder = new StringBuilder();
            for (int i = 0; i < hashed.Length; i++)
            {
                stringbuilder.Append(hashed[i].ToString("x2"));
            }

            return stringbuilder.ToString();
        }
    }
}