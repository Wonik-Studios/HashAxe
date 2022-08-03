using System.Text.Json;


namespace HashAxe.LoadHash {
    // This class will be responsible for loading in all of the hashes from https://virusshare.com/hashes
    class Downloader {
        private const string jsonLink = "hashmap.json";
        private string hashaxeRoot;
        private Dictionary<string, HashList> hashLists;
        
        public Downloader(string hashaxeRoot) {
            this.hashaxeRoot = hashaxeRoot;
            this.hashLists = this.LoadJson();
        }
        
        public Dictionary<string, HashList> LoadJson() {
            string path = Path.Combine(this.hashaxeRoot, jsonLink);

            if (!File.Exists(path))
            {
                FileStream newFile = File.Create(path);
                newFile.Close();
            }

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                List<HashList>? hashLists = JsonSerializer.Deserialize<List<HashList>>(
                    json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );
                
                Dictionary<string, HashList> hashDict = new Dictionary<string, HashList>();
                if(hashLists == null) {
                    return hashDict;
                }
                
                foreach (HashList hashList in hashLists)
                {
                    hashDict.Add(hashList.name, hashList);
                }

                return hashDict;
            }
        }
        
        public void UploadJson(List<HashList> hashLists) {
            string path = Path.Combine(this.hashaxeRoot, jsonLink);
            
            string json = JsonSerializer.Serialize(hashLists);
            File.WriteAllText(path, json);
        }
        
        public Dictionary<string, HashList> GetHashLists() {
            return hashLists;
        }
        
        public class HashList {
            public string name {get; set;}
            public int NUM_HASHES {get; set;}
            public bool enabled {get; set;}
            public string hashlist_source {get; set;}
            public string hashlist_integrity {get; set;}
            public string hashset_source {get; set;}
        }
    }
}