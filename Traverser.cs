using HashAxe.MD5HashSet;
using System.Security.Cryptography;


namespace HashAxe.FileTraverser
{
    public class Traverser {
        private string path { get; set; }
        private MD5Hash hashSet;
        private List<string> flagged = new List<string>();
        private MD5 md5;
        private int filesTraversed;


        public Traverser(string path, MD5Hash hashSet, MD5 md5)
        {
            this.path = path;
            this.hashSet = hashSet;
            this.md5 = md5;
        }

        public void Traverse() {
            if(File.Exists(path)) {
                this.TraverseFile(path);
            }
            else if(Directory.Exists(path)) {
                this.TraverseDir(path);
            }
            else {
                throw new FileNotFoundException("The specified file or directory could not be found.");
            }
        }

        private void TraverseDir(string dir) {
            foreach(string d in Directory.GetFiles(dir)) {
                try {
                    TraverseFile(d);
                } catch(System.Exception e){}
            }

            foreach(string d in Directory.GetDirectories(dir)) {
                try {
                    TraverseDir(d);
                } catch(System.Exception e) {}
            }
        }

        private void TraverseFile(string file) {
            string hash = this.GetMD5(file);
            if(hashSet.Contains(hash)) {
                flagged.Add(file);
            }
            filesTraversed++;
            Console.WriteLine(file + " : " +  hash);
        }

        private string GetMD5(string file) {
            using(FileStream fs = File.OpenRead(file)) {
                byte[] bitHash = md5.ComputeHash(fs);
                return BitConverter.ToString(bitHash).Replace("-", "").ToLowerInvariant();
            }
        }

        public List<String> GetFlagged() {
            return this.flagged;
        }
        /*
        public static void Main(string[] args) {
            Traverser traverser;
            using(FileStream fs = File.Create("data/hashes.dat")) {
                MD5Hash hashSet = new MD5Hash(6, fs);
                hashSet.UploadHash("2d75cc1bf8e57872781f9cd04a529256");
                hashSet.UploadHash("00f538c3d410822e241486ca061a57ee");
                hashSet.UploadHash("3f066dd1f1da052248aed5abc4a0c6a1");
                hashSet.UploadHash("781770fda3bd3236d0ab8274577dddde");
                hashSet.UploadHash("86b6c59aa48a69e16d3313d982791398");
                // This is the hash for rawr.dat
                hashSet.UploadHash("ef3a3971679c0368039d909527cdb972");

                using(MD5 md5 = MD5.Create()) {
                    Console.WriteLine();
                    Console.WriteLine("This is a list of all the files that have been traversed along with their md5 hashes:");
                    Console.WriteLine("----------------------------------------------------");
                    traverser = new Traverser("/Users/nathankim/Documents/C#Projects/HashAxe/test", hashSet, md5);
                    traverser.Traverse();
                }
            }

            Console.WriteLine();
            Console.WriteLine("These are the files that have been flagged:");
            Console.WriteLine("----------------------------------------------------");
            foreach(string file in traverser.GetFlagged()) {
                Console.WriteLine(file);
            }
        }
        */
    }
}