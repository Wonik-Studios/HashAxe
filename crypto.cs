using System.Text;
using System.Security.Cryptography;

namespace HashAxe.Crypto
{
    class Hash
    {
        public static string sha256(string data)
        {
            SHA256 sha256 = SHA256.Create();
            byte[] rawData = Encoding.UTF8.GetBytes(data);
            byte[] hashed = sha256.ComputeHash(rawData);

            return hex(hashed);
        }

        public static string hex(byte[] data){
            StringBuilder hexData = new StringBuilder();

            for (int i = 0; i < data.Length; i++)
            {
                hexData.Append(data[i].ToString("x2"));
            }

            return hexData.ToString();
        }
    }
}