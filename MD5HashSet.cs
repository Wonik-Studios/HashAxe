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
        private Stream stream;


        public MD5Hash(Stream stream) {
            this.stream = stream;
            byte[] buffer = new byte[4];
            this.stream.Read(buffer, 0, buffer.Length);
            this.NUM_HASHES = BitConverter.ToInt32(buffer);
            this.HASHLIST_LENGTH = NextPrime(this.NUM_HASHES * 10 + 1);
        }

        public MD5Hash(int NUM_HASHES, Stream stream) {
            this.stream = stream;
            this.NUM_HASHES = NUM_HASHES;
            this.HASHLIST_LENGTH = NextPrime(this.NUM_HASHES * 10 + 1);
        }
        
        private static long Power(int b, int e) {
            long ans = 1;
            for(int i=0; i < e; i++) {
                ans *= b;
            }
            return ans;
        }

        private static int NextPrime(int number) {
            // number = number % 2 == 0 ? number + 1 : number;
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
            byte[] writeSize = BitConverter.GetBytes(NUM_HASHES);
            stream.Write(writeSize, 0, writeSize.Length);
            
            for(int i=0; i < 32; i++) {
                byte[] buffer = new byte[HASHLIST_LENGTH];
                stream.Write(buffer, 0, buffer.Length);
            }
            stream.Position = 0;
        }

        public void UploadHash(byte[] md5) {
            int hash = this.HashMD5(md5);
            byte[] buffer = new byte[32];
            int inc = 0;

            do {
                int pos = (hash + inc * inc) % HASHLIST_LENGTH;

                stream.Seek(pos * 32L + 4, SeekOrigin.Begin);
                stream.Read(buffer, 0, buffer.Length);
                inc++;
            } while(buffer[0] != 0);

            stream.Position -= 32;
            stream.Write(md5, 0, buffer.Length);
        }

        public bool Contains(byte[] md5) {
            byte[] buffer = new byte[32];
            int hash = this.HashMD5(md5);
            int inc = 0;
            while(true) {
                int pos = (hash + inc * inc) % HASHLIST_LENGTH;

                stream.Seek(pos * 32L + 4, SeekOrigin.Begin);
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
            return (int) (BitConverter.ToInt64(md5, 24) % HASHLIST_LENGTH);
        }

        public int GetHashListLength() {
            return HASHLIST_LENGTH;
        }

        
        // public static void Main(string[] args) {
        //     Console.WriteLine("Program Started.");

        //     using(FileStream stream = File.Create("data/virus_share.dat")) {
        //         MD5Hash hl = new MD5Hash(14, stream);
        //         hl.FillHashes();

        //         byte[] myHash = Encoding.UTF8.GetBytes("aeb3f30120dbde68a3836d8350e503e0");
        //         byte[] myHash1 = Encoding.UTF8.GetBytes("47bb46611bc914709786208100ab7e7a");
        //         byte[] myHash2 = Encoding.UTF8.GetBytes("52f215d357b43c41eabfb9c1db82170a");
                
        //         hl.UploadHash(myHash);
        //         hl.UploadHash(myHash1);
        //         hl.UploadHash(myHash2);
                
        //         Console.WriteLine(hl.Contains(myHash));
        //         Console.WriteLine(hl.Contains(myHash1));
        //         Console.WriteLine(hl.Contains(myHash2));
        //         Console.WriteLine(hl.Contains(Encoding.UTF8.GetBytes("f461518ec34073ccd4517feadecd67ba")));
        //     }
        //     Console.WriteLine("The program has Terminated");
        // }
        
    }
}
