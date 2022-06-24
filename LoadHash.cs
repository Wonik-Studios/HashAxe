namespace HashAxe.LoadHash
{
    class HashLoader
    {
        // the HASHLIST_LENGTH is the number of lines in the hashes.txt file. This number should be 1.3 times the total number of hashes.
        const int HASHLIST_LENGTH = 49999991;

        // This is the Hashunction that will be used to determine which line of hashes.txt it will occupy.
        public int HashMD5(string md5)
        {
            return Convert.ToInt32(md5.Substring(0, 7), 16) % HASHLIST_LENGTH;
        }

        
    }
}