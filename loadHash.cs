class HashLoader {
    // the HASHLIST_LENGTH is the number of lines in the hashes.txt file. This number should be 1.3 times the total number of hashes.
    long HASHLIST_LENGTH;

    // This is the Hashing function that will be used to determine which line of hashes.txt it will occupy.
    public long HashMD5(string md5) {
        return Convert.ToInt64(md5.Substring(0, 7), 16) % HASHLIST_LENGTH;
    }

    
}