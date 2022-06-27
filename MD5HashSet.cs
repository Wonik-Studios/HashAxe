using System.Text;


namespace HashAxe.MD5HashSet
{
    public class MD5Hash
    {
        // NUM_HASHES: The name speaks for itself. It's the number of hashes that we are getting from virusshare.
        // HASHLIST_LENGTH: The number of nodes we have that can contain md5 hashes.
        // NUM_HEXADECIMAL: This is the number of hexadecimal digits that we take from the 
        private readonly int NUM_HASHES;
        private readonly int HASHLIST_LENGTH;
        private readonly int NUM_HEXADECIMAL;
        private Stream stream;

        public MD5Hash(int NUM_HASHES, Stream stream) {
            this.stream = stream;
            this.NUM_HASHES = NUM_HASHES;
            this.HASHLIST_LENGTH = NextPrime(this.NUM_HASHES * 2 + 1);
            Console.WriteLine("This is the HASHLIST_LENGTH: " + this.HASHLIST_LENGTH);

            int i = 0;
            while(HASHLIST_LENGTH > Math.Pow(16, i)) {
                i++;
            }
            this.NUM_HEXADECIMAL = i;
            Console.WriteLine("NUM_HEXADECIMAL: " + this.NUM_HEXADECIMAL);
        }

        private static int NextPrime(int number) {
            while(true) {
                bool isPrime = true;
                int squaredNumber = (int) Math.Sqrt(number);

                for (int i = 2; i <= squaredNumber; i++) {
                    if (number % i == 0) {
                        isPrime = false;
                        break;
                    }
                }
                if(isPrime) {
                    return number;
                }
                number = number + 2;
            }
        }

        public void FillHashes() {
            for(int i=0; i < 32; i++) {
                byte[] buffer = new byte[HASHLIST_LENGTH];
                stream.Write(buffer, 0, buffer.Length);
            }
            stream.Position = 0;
            Console.WriteLine("Filled hashes.dat with empty hashes.");
        }

        public void UploadHash(string md5) {
            int hash = this.HashMD5(md5);
            byte[] buffer = new byte[32];
            int inc = 0;

            do {
                int pos = (hash + inc * inc) % HASHLIST_LENGTH;

                stream.Seek(pos * 32L, SeekOrigin.Begin);
                stream.Read(buffer, 0, buffer.Length);
                inc++;
            } while(buffer[0] != 0);

            stream.Position -= 32;
            stream.Write(Encoding.ASCII.GetBytes(md5), 0, buffer.Length);
        }

        public bool Contains(string md5) {
            byte[] hashAscii = Encoding.ASCII.GetBytes(md5);
            byte[] buffer = new byte[32];
            int hash = this.HashMD5(md5);
            int inc = 0;
            while(true) {
                int pos = (hash + inc * inc) % HASHLIST_LENGTH;

                stream.Seek(pos * 32L, SeekOrigin.Begin);
                stream.Read(buffer, 0, buffer.Length);
                if(buffer[0] == 0) {
                    return false;
                }
                else if(Enumerable.SequenceEqual<byte>(buffer, hashAscii)) {
                    return true;
                }
                inc++;
            }
        }

        // This is the Hash function that will be used to determine which line of hashes.txt it will occupy.
        private int HashMD5(string md5) {
            return Convert.ToInt32(md5.Substring(0, NUM_HEXADECIMAL), 16) % HASHLIST_LENGTH;
        }

        public int GetHashListLength() {
            return HASHLIST_LENGTH;
        }
/*
        public static void Main(string[] args) {
            Console.WriteLine("Program Started.");

            using(FileStream stream = File.Create("data/hashes.dat")) {
                MD5HashSet hl = new MD5HashSet(10^4, stream);
                hl.FillHashes();

                string myHash = "2d75cc1bf8e57872781f9cd04a529256";
                string myHash1 = "2d75cc1bf8e57872781f9cd04a529258";

                hl.UploadHash(myHash);
                hl.UploadHash(myHash1);

                Console.WriteLine(hl.Contains(myHash));
                Console.WriteLine(hl.Contains(myHash1));
                Console.WriteLine(hl.Contains("2d75cc1bf8e57872781f9cd04a52925f"));
                Console.WriteLine(hl.Contains("00f538c3d410822e241486ca061a57ee"));
            }
            Console.WriteLine("The program has Terminated");
        }
*/
    }
}