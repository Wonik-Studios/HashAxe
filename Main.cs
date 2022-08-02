using System.CommandLine;
using System.Reflection;
using System.Security.Cryptography;
using ModifiedOutput;
using HashAxe.FileTraverser;
using HashAxe.MD5HashSet;
using HashAxe.LoadHash;


namespace HashAxe
{
    class HashAxeMain
    {
        private static Dictionary<string, Downloader.HashList> hashLists;

        static async Task Main(string[] args)
        {
            Command checksum = new Command("checksum", "Checks for remote updates on the hashlists and makes sure locally stored hashsets have not been corrupted");
            Command listHashets = new Command("hashsets", "List all the installed hashsets in the configuration");
            Command downloadHashset = new Command("hashset-get", "Install a hashset from a hashlist url");
            Command removeHashet = new Command("hashset-remove", "Uninstalls a hashset from the configuration");
            Command disableHashset = new Command("hashet-off", "Disables a hashset so it won't be included in the scan");
            Command enableHashset = new Command("hashet-on", "Enables a previously disabled hashset");
            Command traverse = new Command("traverse", "Scans the speficied path");

            RootCommand rCommand = new RootCommand("This is the root command for HashAxe made by Wonik. If you are seeing this, that means HashAxe is installed properly.");

            // downloadHashset arguments
            Argument<String> hashlistUrlArg = new Argument<String>("Hashlist Source", "Http link to the source file of the hashlist");
            Option<String> integrityArg = new Option<string>("--integrity", "Optional parameter that ensures the content of the downloaded file. SHA256");
            Option<String> customNameArg = new Option<string>("--setname", "Optional parameter which disregards name listed in hashlist and allows you to supply your own name for the HashSet");

            downloadHashset.Add(hashlistUrlArg);
            downloadHashset.Add(integrityArg);
            downloadHashset.Add(customNameArg);

            rCommand.Add(checksum);
            rCommand.Add(listHashets);
            rCommand.Add(downloadHashset);
            rCommand.Add(removeHashet);
            rCommand.Add(disableHashset);
            rCommand.Add(enableHashset);
            rCommand.Add(traverse);

            string launchPath = root();
            Downloader downloader = new Downloader(launchPath);
            hashLists = downloader.GetHashLists();

            checksum.SetHandler(async () =>
            {
                await Cmd_Checksum(launchPath);
            });

            listHashets.SetHandler(async () =>
            {
                await Cmd_ListHashSets(launchPath);
            });

            downloadHashset.SetHandler(async (hashlist_url, integrity, customName) =>
            {
                await Cmd_DownloadHashset(launchPath, hashlist_url, integrity, customName);
            }, hashlistUrlArg, integrityArg, customNameArg);

            removeHashet.SetHandler(async () =>
            {
                await Cmd_RemoveHashset(launchPath);
            });

            disableHashset.SetHandler(async () =>
            {
                await Cmd_DisableHashset(launchPath);
            });

            enableHashset.SetHandler(async () =>
            {
                await Cmd_EnableHashset(launchPath);
            });

            traverse.SetHandler(async () =>
            {
                await Cmd_Traverse(launchPath);
            });

            await rCommand.InvokeAsync(args);
        }

        internal static string root()
        {
            string hashaxe_root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "hashaxe");
            string hashsets_root = Path.Combine(hashaxe_root, "hashsets");

            LineOutput.WriteLineColor("HashAxe {0}", ConsoleColor.Green, Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            LineOutput.WriteLineColor("Brought to you by the lovely members of Wonik", ConsoleColor.Green);

            try
            {
                if (!Directory.Exists(hashaxe_root))
                {
                    Directory.CreateDirectory(hashaxe_root);
                    Console.WriteLine("Initalized empty HashAxe configuration directory at {0}", hashaxe_root);
                }

                if (!Directory.Exists(hashsets_root))
                {
                    Directory.CreateDirectory(hashsets_root);
                    Console.WriteLine("Initalized empty HashSets store directory at {0}", hashsets_root);
                }
            }
            catch (Exception e)
            {
                LineOutput.WriteLineColor("\nThe root handler encountered an error, {0}", ConsoleColor.Red, e.Message);
                Environment.Exit(1);
            }

            return hashaxe_root;
        }

        internal static async Task Cmd_Checksum(string hashaxe_root)
        {
            Console.WriteLine(hashaxe_root);
            return;
        }

        internal static async Task Cmd_DownloadHashset(string hashaxe_root, string hashlist_url, string? integrity = null, string? customName = null)
        {
            Console.WriteLine("{0} {1} {2}", hashlist_url, integrity, customName);
            return;
        }

        internal static async Task Cmd_RemoveHashset(string hashaxe_root)
        {
            Console.WriteLine(hashaxe_root);
            return;
        }

        internal static async Task Cmd_DisableHashset(string hashaxe_root)
        {
            Console.WriteLine(hashaxe_root);
            return;
        }

        internal static async Task Cmd_EnableHashset(string hashaxe_root)
        {
            Console.WriteLine(hashaxe_root);
            return;
        }

        internal static async Task Cmd_ListHashSets(string hashaxe_root)
        {
            foreach (Downloader.HashList entry in hashLists.Values)
            {
                Console.WriteLine("-------------------------------------------------------------------------");
                Console.WriteLine("NAME         | {0}", entry.name);
                Console.WriteLine("HASHLIST SRC | {0}", entry.hashlist_source);
                Console.WriteLine("INTEGRITY    | {0}", entry.hashlist_integrity);
                Console.WriteLine("# OF HASHES  | {0}", entry.NUM_HASHES);
                Console.WriteLine("ENABLED      | {0}", entry.enabled ? "YES" : "NO");
            }
            Console.WriteLine("-------------------------------------------------------------------------");
            return;
        }

        internal static async Task Cmd_Traverse(string hashaxe_root)
        {
            Traverser traverser;
            List<string> flagged = new List<string>();
            
            foreach(Downloader.HashList hashList in hashLists.Values) {
                if(hashList.enabled) {
                    try {
                        using (FileStream fs = File.OpenRead(Path.Combine(hashaxe_root, "hashsets", hashList.hashset_source)))
                        {
                            MD5Hash hashSet = new MD5Hash(hashList.NUM_HASHES, fs);
                            using (MD5 md5 = MD5.Create())
                            {
                                traverser = new Traverser("/Users/nathankim/Documents/C#Projects/HashAxe/test", hashSet, md5);
                                traverser.Traverse();
                            }
                        }
                    } catch(Exception e) {
                        Console.WriteLine("An Error has stopped the hashList {0} from being properly checked against the files.");
                    }
                }
                
            }

            Console.WriteLine();
            Console.WriteLine("These are the paths for the " + flagged.Count + " files that have been flagged:");
            Console.WriteLine("----------------------------------------------------");
            foreach (string file in flagged)
            {
                Console.WriteLine(file);
            }
            return;
        }
    }
}
