using System.CommandLine;
using System.Reflection;
using ModifiedOutput;
using HashAxe.FileTraverser;
using HashAxe.MD5HashSet;
using HashAxe.LoadHash;
using System.Security.Cryptography;


#pragma warning disable CS8600
#pragma warning disable CS8604

namespace HashAxe
{
    class HashAxeMain
    {   
        static async Task Main(string[] args)
        {
            Downloader loadHash = new Downloader("link.json");
            
            
            Command checksum = new Command("checksum", "Checks for remote updates on the hashlists and makes sure locally stored hashsets have not been corrupted");
            Command listHashets = new Command("hashsets", "List all the installed hashsets in the configuration");
            Command downloadHashet = new Command("hashset-get", "Install a hashset from a hashlist url");
            Command removeHashet = new Command("hashset-remove", "Uninstalls a hashset from the configuration");
            Command disableHashset = new Command("hashet-off", "Disables a hashset so it won't be included in the scan");
            Command enableHashset = new Command("hashet-on", "Enables a previously disabled hashset");
            Command traverse = new Command("traverse", "Scans the speficied path");

            RootCommand rCommand = new RootCommand("This is the root command for HashAxe made by Wonik. If you are seeing this, that means HashAxe is installed properly.");

            rCommand.Add(checksum);
            rCommand.Add(listHashets);
            rCommand.Add(downloadHashet);
            rCommand.Add(removeHashet);
            rCommand.Add(disableHashset);
            rCommand.Add(enableHashset);
            rCommand.Add(traverse);

            checksum.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_Checksum(launchPath);
            });

            listHashets.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_ListHashes(launchPath);
            });

            downloadHashet.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_DownloadHashset(launchPath);
            });

            removeHashet.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_RemoveHashset(launchPath);
            });

            disableHashset.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_DisableHashset(launchPath);
            });

            enableHashset.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_EnableHashset(launchPath);
            });

            traverse.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_Traverse(launchPath);
            });

            await rCommand.InvokeAsync(args);
        }
        internal static string root()
        {
            LineOutput.WriteLineColor("HashAxe {0}", ConsoleColor.Green, Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            LineOutput.WriteLineColor("Brought to you by the lovely members of Wonik", ConsoleColor.Green);
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "hashaxe");
        }

        internal static async Task Cmd_Checksum(string hashaxe_root)
        {
            Console.WriteLine(hashaxe_root);
            return;
        }

        internal static async Task Cmd_ListHashes(string hashaxe_root)
        {
            Console.WriteLine(hashaxe_root);
            return;
        }

        internal static async Task Cmd_DownloadHashset(string hashaxe_root){
            Console.WriteLine(hashaxe_root);
            return;
        }

        internal static async Task Cmd_RemoveHashset(string hashaxe_root){
            Console.WriteLine(hashaxe_root);
            return;
        }

        internal static async Task Cmd_DisableHashset(string hashaxe_root){
            Console.WriteLine(hashaxe_root);
            return;
        }

        internal static async Task Cmd_EnableHashset(string hashaxe_root){
            Console.WriteLine(hashaxe_root);
            return;
        }
        
        internal static async Task Cmd_ListHashSets() {
            Console.WriteLine("---------------------------------------------------------------------------");
            Console.WriteLine("| Hash Set Name                                                           |");
            Console.WriteLine("---------------------------------------------------------------------------");
            foreach(KeyValuePair<string, int> entry in hashSets) {
                Console.WriteLine(String.Format("| {0,-50}| {1,-20}|", entry.Key, entry.Value));
            }
            Console.WriteLine("---------------------------------------------------------------------------");
            return;
        }

        internal static async Task Cmd_Traverse(string hashaxe_root)
        {
            Traverser traverser;
            using (FileStream fs = File.OpenRead("data/hashes.dat"))
            {
                MD5Hash hashSet = new MD5Hash(6, fs);
                using (MD5 md5 = MD5.Create())
                {
                    traverser = new Traverser("/Users/nathankim/Documents/C#Projects/HashAxe/test", hashSet, md5);
                    traverser.Traverse();
                }
            }

            Console.WriteLine();
            Console.WriteLine("These are the paths for the " + traverser.GetFlagged().Count + " files that have been flagged:");
            Console.WriteLine("----------------------------------------------------");
            foreach (string file in traverser.GetFlagged())
            {
                Console.WriteLine(file);
            }
            return;
        }
    }
}