using HashAxe.MD5HashSet;
using System.Security.Cryptography;
using System.Text;
using HashAxe.Crypto;
using HashAxe.ModifiedOutput;


namespace HashAxe.FileTraverser
{
    public class Traverser
    {
        private long completedFiles;
        private string path { get; set; }
        private MD5Hash hashSet;
        private List<string> flagged = new List<string>();
        private MD5 md5;
        private int filesTraversed;
        private bool verboseLogging;


        public Traverser(string path, MD5Hash hashSet, MD5 md5, bool verboseLogging)
        {
            this.path = Path.GetFullPath(path);
            this.hashSet = hashSet;
            this.md5 = md5;
            this.verboseLogging = verboseLogging;
        }

        public void Traverse()
        {
            if (File.Exists(path))
            {
                this.TraverseFile(path);
            }
            else if (Directory.Exists(path))
            {
                this.TraverseDir(path);
            }
            else
            {
                throw new FileNotFoundException("The specified file or directory could not be found.");
            }
        }

        private void TraverseDir(string dir)
        {
            try
            {
                foreach (string d in Directory.GetFiles(dir))
                {
                    try
                    {
                        TraverseFile(d);
                    }
                    catch (Exception) { }
                }

                foreach (string d in Directory.GetDirectories(dir))
                {
                    try
                    {
                        TraverseDir(d);
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception)
            {
                LineOutput.LogWarning("Failed to traverse directory {0}", dir);
            }
        }

        private void TraverseFile(string file)
        {
            try
            {

                byte[] hash = this.GetMD5(file);
                string hexHash = Hash.hex(hash);

                if (verboseLogging)
                {
                    Console.Write($"\r{file} :: ");
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(hexHash);
                    Console.ForegroundColor = ConsoleColor.White;
                }

                completedFiles++;
                Console.Write("\rFiles completed: {0}", completedFiles);

                if (hashSet.Contains(Encoding.UTF8.GetBytes(hexHash)))
                {
                    flagged.Add(file);
                }
                filesTraversed++;
            }
            catch (Exception)
            {
                LineOutput.LogWarning("Failed to check the file {0}", file);
            }
        }

        private byte[] GetMD5(string file)
        {
            using (FileStream fs = File.OpenRead(file))
            {
                return md5.ComputeHash(fs);
            }
        }

        public List<String> GetFlagged()
        {
            return this.flagged;
        }
    }
}