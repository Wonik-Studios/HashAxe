using System.CommandLine;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using HashAxe.Crypto;
using HashAxe.ModifiedOutput;
using HashAxe.FileTraverser;
using HashAxe.MD5HashSet;
using HashAxe.LoadHash;


namespace HashAxe
{
    class HashAxeMain
    {
        static readonly HttpClient client = new HttpClient();
        private static Dictionary<string, Downloader.HashList> hashLists;
        private static Downloader downloader;

        static async Task Main(string[] args)
        {
            // Initialize the Commands
            Command checksum = new Command("checksum", "Checks for remote updates on the hashlists and makes sure locally stored hashsets have not been corrupted");
            Command listHashets = new Command("hashsets", "List all the installed hashsets in the configuration");
            Command downloadHashset = new Command("hashset-get", "Install a hashset from a hashlist url");
            Command removeHashet = new Command("hashset-remove", "Uninstalls a hashset from the configuration");
            Command disableHashset = new Command("hashset-off", "Disables a hashset so it won't be included in the scan");
            Command enableHashset = new Command("hashset-on", "Enables a previously disabled hashset");
            Command traverse = new Command("traverse", "Scans the speficied path");

            // The root command.
            RootCommand rCommand = new RootCommand("This is the root command for HashAxe made by Wonik. If you are seeing this, that means HashAxe is installed properly.");

            // Set the arguments for the commands.
            Argument<string> hashlistUrlArg = new Argument<string>("Hashlist Source", "Http link to the source file of the hashlist");
            Option<string> integrityArg = new Option<string>("--integrity", "Optional parameter that ensures the content of the downloaded file. SHA256");
            Option<string> customNameArg = new Option<string>("--setname", "Optional parameter which disregards name listed in hashlist and allows you to supply your own name for the HashSet");

            downloadHashset.Add(hashlistUrlArg);
            downloadHashset.Add(integrityArg);
            downloadHashset.Add(customNameArg);
            
            Argument<string> searchPathArg = new Argument<string>("search-path", "The directory or file that will be traversed and checked for any flagged malware according to the enabled hashsets.");
            traverse.Add(searchPathArg);
            
            Argument<string> nameEnableArg = new Argument<string>("hashlist-name", "The name of the hashlist that will be enabled.");
            enableHashset.Add(nameEnableArg);
            
            Argument<string> nameDisableArg = new Argument<string>("hashlist-name", "The name of the hashlist that will be disabled.");
            disableHashset.Add(nameDisableArg);
            
            Argument<string> nameRemoveArg = new Argument<string>("hashlist-name", "The name of the hashlist to be removed.");
            removeHashet.Add(nameRemoveArg);

            rCommand.Add(checksum);
            rCommand.Add(listHashets);
            rCommand.Add(downloadHashset);
            rCommand.Add(removeHashet);
            rCommand.Add(disableHashset);
            rCommand.Add(enableHashset);
            rCommand.Add(traverse);

            // Initialize the hashaxe_root directory as "launchPath", the hashLists which will be responsible for holding data about the hashsets and the downloader.
            string launchPath = root();
            downloader = new Downloader(launchPath);
            hashLists = downloader.GetHashLists();

            checksum.SetHandler(async () =>
            {
                await Cmd_Checksum(launchPath);
            });

            listHashets.SetHandler(async () =>
            {
                await Cmd_ListHashSets(launchPath);
            });

            downloadHashset.SetHandler(async (string hashlist_url, string? integrity, string? customName) =>
            {
                await Cmd_DownloadHashset(launchPath, hashlist_url, integrity, customName);
            }, hashlistUrlArg, integrityArg, customNameArg);

            removeHashet.SetHandler(async (string name) =>
            {
                await Cmd_RemoveHashset(launchPath, name);
            }, nameRemoveArg);

            disableHashset.SetHandler(async (string name) =>
            {
                await Cmd_DisableHashset(name);
            }, nameDisableArg);

            enableHashset.SetHandler(async (string name) =>
            {
                Console.WriteLine(name);
                await Cmd_EnableHashset(name);
            }, nameEnableArg);

            traverse.SetHandler(async (string searchPath) =>
            {
                await Cmd_Traverse(launchPath, searchPath);
            }, searchPathArg);

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
            try{
                if (!hashlist_url.StartsWith("http://") && !hashlist_url.StartsWith("https://")){
                    LineOutput.WriteLineColor("The Hashlist source URL is invalid.", ConsoleColor.Red);
                    return;
                }

                HttpResponseMessage response = await client.GetAsync(hashlist_url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                
                if (!String.IsNullOrEmpty(responseBody)) {
                    responseBody = responseBody.Substring(0, responseBody.Length - 1);
                } else {
                    throw new Exception("The hashlist on the URL is empty.");
                }
                
                if (!String.IsNullOrEmpty(integrity)){
                    string checksum = Hash.sha256(responseBody);
                    integrity = integrity.Trim();
                    Console.WriteLine(responseBody);
                    Console.WriteLine("{0} {1}", checksum, integrity);
                }
                else{
                    LineOutput.WriteLineColor("An integrity hash was not supplied. This is highly reccomended.", ConsoleColor.Yellow);
                }
            }
            catch(Exception e){
                LineOutput.WriteLineColor("\nhashset-get encountered an error", ConsoleColor.Red);
                LineOutput.WriteLineColor(e.Message, ConsoleColor.Red);
            }
        }

        internal static async Task Cmd_RemoveHashset(string hashaxe_root, string name)
        {
            Regex regex = new Regex(@"(?i)y(es)?");
            Console.Write("Are you sure that you want to delete the hash set {0}? [Y/n]: ", name);
            string response = Console.ReadLine();
            
            if(!regex.IsMatch(response)) {
                Console.WriteLine("Exiting out of Command...");
                return;
            }
            
            if(hashLists.ContainsKey(name)){
                Console.WriteLine("The hash set under the name {0} does not exist.", name);
            } else {
                Downloader.HashList toRemove = hashLists[name];
                string path = Path.Combine(hashaxe_root, "hashsets", toRemove.hashset_source);
                File.Delete(path);
                
                hashLists.Remove(name);
                try {
                    downloader.UploadJson(hashLists.Values.ToList());
                    Console.WriteLine("The hash set {0} has successfully been removed.", name);
                    
                } catch(Exception e) {
                    Console.WriteLine("There was an unexpected error when attempting to remove the hashset {0}.", name);
                    Console.WriteLine("Error Message: " + e.Message);
                }
            }
            return;
        }

        internal static async Task Cmd_DisableHashset(string name)
        {
            if(hashLists.ContainsKey(name)) {
                Downloader.HashList toDisable = hashLists[name];
                toDisable.enabled = false;
            } else {
                Console.WriteLine("Failed to find a hash set under the name {0}.", name);
                return;
            }
            
            try {
                downloader.UploadJson(hashLists.Values.ToList());
                Console.WriteLine("HashAxe has successfully disabled the hash set {0}.", name);
            } catch(Exception e) {
                Console.WriteLine("There was an unexpected error when attempting to disable the hashset {0}.", name);
                Console.WriteLine("Error Message: " + e.Message);
            }
            return;
        }

        internal static async Task Cmd_EnableHashset(string name)
        {
            Console.WriteLine("Name: " + name);
            if(hashLists.ContainsKey(name)) {
                Downloader.HashList toEnable = hashLists[name];
                toEnable.enabled = true;
            } else {
                Console.WriteLine("Failed to find a hash set under the name {0}.", name);
            }
            
            try {
                downloader.UploadJson(hashLists.Values.ToList());
            } catch(Exception e) {
                Console.WriteLine("There was an unexpected error when attempting to enable the hashset {0}.", name);
                Console.WriteLine("Error Message: " + e.Message);
                return;
            }
            Console.WriteLine("HashAxe has successfully enabled the hash set {0}.", name);
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

        internal static async Task Cmd_Traverse(string hashaxe_root, string searchPath)
        {
            Traverser traverser;
            HashSet<string> flagged = new HashSet<string>();
            
            foreach(Downloader.HashList hashList in hashLists.Values) {
                if(hashList.enabled) {
                    try {
                        using (FileStream fs = File.OpenRead(Path.Combine(hashaxe_root, "hashsets", hashList.hashset_source)))
                        {
                            MD5Hash hashSet = new MD5Hash(hashList.NUM_HASHES, fs);
                            using (MD5 md5 = MD5.Create())
                            {
                                traverser = new Traverser(searchPath, hashSet, md5);
                                traverser.Traverse();
                            }
                        }
                        flagged.UnionWith(traverser.GetFlagged());
                    } catch(Exception e) {
                        Console.WriteLine("An Error has stopped the hashList {0} from being properly checked against the files.", hashList.name);
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