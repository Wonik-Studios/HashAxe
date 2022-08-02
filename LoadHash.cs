using Newtonsoft.Json;


namespace HashAxe.LoadHash {
    // This class will be responsible for loading in all of the hashes from https://virusshare.com/hashes
    class Downloader {
        private string jsonLink;
        private Dictionary<string, HashList> hashLists;
        
        public Downloader(string jsonLink) {
            this.jsonLink = jsonLink;
            this.hashLists = LoadJson(this.jsonLink);
        }
        
        public static Dictionary<string, HashList> LoadJson(string jsonLink) {
            using (StreamReader r = new StreamReader("file.json"))
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
        
        public string GetJsonLink() {
            return this.jsonLink;
        }
        
        public void SetJsonLink(string jsonLink) {
            this.jsonLink = jsonLink;
        }
        
        public Dictionary<string, HashList> GetHashLists() {
            return hashLists;
        }
        
        public class HashList {
            public string name;
            public int NUM_HASHES;
            public bool enabled;
            public string integrity;
            
            public List<string> urls;
        }
    }
}