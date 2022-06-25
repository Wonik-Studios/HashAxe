using System;
using System.IO;
using System.Text;


namespace HashAxe.MD5HashSet
{
    class MD5HashSet
    {
        // NUM_HASHES: The name speaks for itself. It's the number of hashes that we are getting from virusshare.
        // HASHLIST_LENGTH: The number of nodes we have that can contain md5 hashes.
        // NUM_HEXADECIMAL: This is the number of hexadecimal digits that we take from the 
        private readonly int NUM_HASHES;
        private readonly int HASHLIST_LENGTH;
        private readonly int NUM_HEXADECIMAL;

        public MD5HashSet(int NUM_HASHES) {
            this.NUM_HASHES = NUM_HASHES;
            this.HASHLIST_LENGTH = MD5HashSet.NextPrime(this.NUM_HASHES * 2 + 1);
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

        public void FillHashes(FileStream fs) {
            fs.Write(new byte[NUM_HASHES * 16], 0, NUM_HASHES * 16);
        }

        public void UploadHash(FileStream fs, string md5) {
            int hash = this.HashMD5(md5);
            byte[] buffer = new byte[NUM_HEXADECIMAL];
            int inc = 0;

            do {
                fs.Read(buffer, (hash + inc * inc) % HASHLIST_LENGTH, NUM_HEXADECIMAL);
                inc++;
            } while(buffer[0] != 0);
            fs.Write(Encoding.ASCII.GetBytes(md5));
        }

        public bool Contains(FileStream fs, string md5) {
            byte[] hashAscii = Encoding.ASCII.GetBytes(md5);
            byte[] buffer = new byte[NUM_HEXADECIMAL];
            int hash = this.HashMD5(md5);
            int inc = 0;
            while(true) {
                fs.Read(buffer, hash + inc * inc, NUM_HEXADECIMAL);
                if(buffer[0] == 0) {
                    return false;
                }
                else if(Enumerable.SequenceEqual<Byte>(buffer, hashAscii)) {
                    return true;
                }
                inc++;
            }
        }

        public void WriteHashes() {
            
        }

        // This is the Hashunction that will be used to determine which line of hashes.txt it will occupy.
        public int HashMD5(string md5)
        {
            return Convert.ToInt32(md5.Substring(0, NUM_HEXADECIMAL), 16) % HASHLIST_LENGTH;
        }

        public int GetHashListLength() {
            return HASHLIST_LENGTH;
        }

        public static void Main(string[] args) {
            Console.WriteLine("Program Started.");

            MD5HashSet hl = new MD5HashSet(20);
            using(FileStream fs = File.Create("data/hashes.dat")) {
                hl.FillHashes(fs);
            }
        }
    }
}