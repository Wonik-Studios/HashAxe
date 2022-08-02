using Newtonsoft.Json;


namespace HashAxe.LoadHash {
    // This class will be responsible for loading in all of the hashes from https://virusshare.com/hashes
    class Downloader {
        private const string jsonLink = "hashmap.json";
        private string hashaxeRoot;
        private Dictionary<string, HashList> hashLists;
        
        public Downloader(string hashaxeRoot) {
            this.hashLists = this.LoadJson();
            this.hashaxeRoot = hashaxeRoot;
        }
        
        public Dictionary<string, HashList> LoadJson() {
            using (StreamReader r = new StreamReader(Path.Combine(this.hashaxeRoot, jsonLink)))
            {
                string json = r.ReadToEnd();
                List<HashList> hashLists = JsonConvert.DeserializeObject<List<HashList>>(json);
                
                Dictionary<string, HashList> hashDict = new Dictionary<string, HashList>();
                foreach(HashList hashList in hashLists) {
                    hashDict.Add(hashList.name, hashList);
                }
                
                return hashDict;
            }
        }
        
        public Dictionary<string, HashList> GetHashLists() {
            return hashLists;
        }
        
        public class HashList {
            public string name;
            public int NUM_HASHES;
            public bool enabled;
            public string hashlist_source;
            public string hashlist_integrity;
            public string hashset_source;
            public List<string> urls;
        }
    }
}