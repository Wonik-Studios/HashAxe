using System.Text.Json;


namespace HashAxe.LoadHash {
    class Downloader {
        private const string jsonLink = "hashmap.json";
        private string launchPath;
        private Dictionary<string, HashList> hashLists;
        private int numFiles = 0;
        private int NUM_HASHES;
        
        
        public Downloader(string launchPath) {
            this.launchPath = launchPath;
            this.hashLists = this.LoadJson();
        }
        
        public Dictionary<string, HashList> LoadJson() {
            string path = Path.Combine(this.launchPath, jsonLink);

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
            string path = Path.Combine(this.launchPath, jsonLink);
            
            string json = JsonSerializer.Serialize(hashLists);
            File.WriteAllText(path, json);
        }
        
        public void DeleteTemp() {
            File.Delete(Path.Combine(this.launchPath, "temp", "swapsource.txt"));
        }
        
        public Dictionary<string, HashList> GetHashLists() {
            return hashLists;
        }
        
        public class HashList {
            public string name {get; set;}
            public int NUM_HASHES {get; set;}
            public bool enabled {get; set;}
            public string hashset_source {get; set;}
            
            public HashList(string name, int NUM_HASHES, bool enabled, string hashset_source) {
                this.name = name;
                this.NUM_HASHES = NUM_HASHES;
                this.enabled = enabled;
                this.hashset_source = hashset_source;
            }
        }
    }
}