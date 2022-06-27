using HashAxe.MD5HashSet;

namespace HashAxe.Traverser
{
    public class Traverser {
        private string path { get; set; }
        private MD5Hash hashSet;

        public Traverser(string path, MD5Hash hashSet)
        {
            if(!(Directory.Exists(path) || File.Exists(path))) {
                throw new FileNotFoundException();
            }
            this.path = path;
            this.hashSet = hashSet;
        }

        public void Traverse() {
            this.Traverse(path);
        }

        private void Traverse(string dir) {
            
        }
    }
}