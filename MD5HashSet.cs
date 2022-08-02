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
        private readonly int NUM_BYTES;
        private Stream stream;

        public MD5Hash(int NUM_HASHES, Stream stream) {
            this.stream = stream;
            this.NUM_HASHES = NUM_HASHES;
            this.HASHLIST_LENGTH = NextPrime(this.NUM_HASHES * 2 + 1);
            Console.WriteLine("This is the HASHLIST_LENGTH: " + this.HASHLIST_LENGTH);

            int i = 0;
            while(HASHLIST_LENGTH > Power(256, i)) {
                i++;
            }
            this.NUM_BYTES = i;
            Console.WriteLine("NUM_BYTES: " + this.NUM_BYTES);
        }
        
        private static long Power(int b, int e) {
            long ans = 1;
            for(int i=0; i < e; i++) {
                ans *= b;
            }
            return ans;
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
            byte[] size = BitConverter.GetBytes(HASHLIST_LENGTH);
            
            for(int i=0; i < 16; i++) {
                byte[] buffer = new byte[HASHLIST_LENGTH];
                stream.Write(buffer, 0, buffer.Length);
            }
            stream.Position = 0;
            Console.WriteLine("Filled hashes.dat with empty hashes.");
        }

        public void UploadHash(byte[] md5) {
            int hash = this.HashMD5(md5);
            byte[] buffer = new byte[16];
            int inc = 0;

            do {
                int pos = (hash + inc * inc) % HASHLIST_LENGTH;

                stream.Seek(pos * 16L, SeekOrigin.Begin);
                stream.Read(buffer, 0, buffer.Length);
                inc++;
            } while(buffer[0] != 0);

            stream.Position -= 16;
            stream.Write(md5, 0, buffer.Length);
        }

        public bool Contains(byte[] md5) {
            byte[] buffer = new byte[16];
            int hash = this.HashMD5(md5);
            int inc = 0;
            while(true) {
                int pos = (hash + inc * inc) % HASHLIST_LENGTH;

                stream.Seek(pos * 16L, SeekOrigin.Begin);
                stream.Read(buffer, 0, buffer.Length);
                if(buffer[0] == 0) {
                    return false;
                }
                else if(Enumerable.SequenceEqual<byte>(buffer, md5)) {
                    return true;
                }
                inc++;
            }
        }

        // This is the Hash function that will be used to determine which line of hashes.txt it will occupy.
        private int HashMD5(byte[] md5) {
            byte[] hashPortion = new byte[16];
            Array.Copy(md5, hashPortion, NUM_BYTES);
            
            return (int) (BitConverter.ToInt64(hashPortion) % HASHLIST_LENGTH);
        }

        public int GetHashListLength() {
            return HASHLIST_LENGTH;
        }
        
        public static byte[] StringToByteArray(String hex) {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        // public static void Main(string[] args) {
        //     Console.WriteLine("Program Started.");

        //     using(FileStream stream = File.Create("data/virus_share.dat")) {
        //         MD5Hash hl = new MD5Hash(10^4, stream);
        //         hl.FillHashes();

        //         byte[] myHash = StringToByteArray("2d75cc1bf8e57872781f9cd04a529256");
        //         byte[] myHash1 = StringToByteArray("2d75cc1bf8e57872781f9cd04a529258");

        //         hl.UploadHash(myHash);
        //         hl.UploadHash(myHash1);

        //         Console.WriteLine(hl.Contains(myHash));
        //         Console.WriteLine(hl.Contains(myHash1));
        //         Console.WriteLine(hl.Contains(StringToByteArray("2d75cc1bf8e57872781f9cd04a52925f")));
        //         Console.WriteLine(hl.Contains(StringToByteArray("00f538c3d410822e241486ca061a57ee")));
        //     }
        //     Console.WriteLine("The program has Terminated");
        // }
    }
}