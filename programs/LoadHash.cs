namespace HashAxe.LoadHash
{
    class HashLoader
    {
        // The name speaks for itself. It's the number of hashes that we are getting from virusshare.
        private static readonly int NUM_HASHES;
        // HASHLIST_LENGTH is the number of nodes we have that can contain md5 hashes.
        private static readonly int HASHLIST_LENGTH;

        static HashLoader() {
            NUM_HASHES = 34;
            HASHLIST_LENGTH = NextPrime(2 * NUM_HASHES + 1);
        }

        public HashLoader() {}

        private static int NextPrime(int number) {    
            while(true) {
                bool isPrime = true;
                //increment the number by 1 each time

                int squaredNumber = (int) Math.Sqrt(number);

                //start at 2 and increment by 1 until it gets to the squared number
                for (int i = 2; i <= squaredNumber; i++) {
                    //how do I check all i's?
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

<<<<<<< HEAD
        public static void FillHashes(FileStream fs) {
            fs.Write(new byte[NUM_HASHES * 16], 0, NUM_HASHES * 16);
        }

        public void uploadHashes() {
            
        }

=======
>>>>>>> 1195b8902a79349bab80355de806c79b34f1d14f
        // This is the Hashunction that will be used to determine which line of hashes.txt it will occupy.
        public int HashMD5(string md5)
        {
            return Convert.ToInt32(md5.Substring(0, 7), 16) % HASHLIST_LENGTH;
        }

<<<<<<< HEAD
        public static void Main(string[] args) {
            using(FileStream fs = File.Create("../hashes.dat")) {
                HashLoader.FillHashes(fs);
            }
=======
        public void Main(string[] args) {
>>>>>>> 1195b8902a79349bab80355de806c79b34f1d14f
        }
    }
}