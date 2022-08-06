using System.CommandLine;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text;

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
        private static string launchPath;

        static async Task Main(string[] args)
        {
            // Initialize the Commands
            Command checksum = new Command("checksum", "Checks for remote updates on the hashlists and makes sure locally stored hashsets have not been corrupted");
            Command listHashets = new Command("hashsets", "List all the installed hashsets in the configuration");
            Command downloadHashset = new Command("hashset-get", "Install a hashset from a hashlist url");
            Command removeHashet = new Command("hashset-remove", "Uninstalls a hashset from the configuration");
            Command disableHashset = new Command("hashset-off", "Disables a hashset so it won't be included in the scan");
            Command enableHashset = new Command("hashset-on", "Enables a previously disabled hashset");
            Command renameHashset = new Command("rename", "Renames a designated hashset");
            Command traverse = new Command("traverse", "Scans the specified path");

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
            
            Argument<string> nameEnableArg = new Argument<string>("hashset-name", "The name of the hashlist that will be enabled.");
            enableHashset.Add(nameEnableArg);
            
            Argument<string> nameDisableArg = new Argument<string>("hashset-name", "The name of the hashlist that will be disabled.");
            disableHashset.Add(nameDisableArg);
            
            Argument<string> nameRemoveArg = new Argument<string>("hashset-name", "The name of the hashlist to be removed.");
            removeHashet.Add(nameRemoveArg);
            
            Argument<string> oldNameArg = new Argument<string>("old-name", "The name of the hashset that will be renamed.");
            Argument<string> newNameArg = new Argument<string>("new-name", "The name that the old hashset will be renamed to.");
            renameHashset.Add(oldNameArg);
            renameHashset.Add(newNameArg);
            
            rCommand.Add(checksum);
            rCommand.Add(listHashets);
            rCommand.Add(downloadHashset);
            rCommand.Add(removeHashet);
            rCommand.Add(disableHashset);
            rCommand.Add(enableHashset);
            rCommand.Add(traverse);
            rCommand.Add(renameHashset);

            // Initialize the launchPath directory as "launchPath", the hashLists which will be responsible for holding data about the hashsets and the downloader.
            root();
            downloader = new Downloader(launchPath);
            hashLists = downloader.GetHashLists();

            checksum.SetHandler(async () =>
            {
                await Cmd_Checksum();
            });

            listHashets.SetHandler(async () =>
            {
                await Cmd_ListHashSets();
            });

            downloadHashset.SetHandler(async (string hashlist_url, string? integrity, string? customName) =>
            {
                await Cmd_DownloadHashset(hashlist_url, integrity, customName);
            }, hashlistUrlArg, integrityArg, customNameArg);

            removeHashet.SetHandler(async (string name) =>
            {
                await Cmd_RemoveHashset(name);
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
            
            renameHashset.SetHandler(async (string oldName, string newName) => {
                await Cmd_RenameHashSet(oldName, newName);
            }, oldNameArg, newNameArg);

            traverse.SetHandler(async (string searchPath) =>
            {
                await Cmd_Traverse(searchPath);
            }, searchPathArg);

            await rCommand.InvokeAsync(args);
        }

        internal static void root()
        {
            launchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "hashaxe");
            string hashsets_root = Path.Combine(launchPath, "hashsets");
            string temp_root = Path.Combine(launchPath, "temp");

            LineOutput.WriteLineColor("HashAxe {0}", ConsoleColor.Green, Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion);
            LineOutput.WriteLineColor("Brought to you by the lovely members of Wonik", ConsoleColor.Green);

            try
            {
                if (!Directory.Exists(launchPath))
                {
                    Directory.CreateDirectory(launchPath);
                    Console.WriteLine("Initalized empty HashAxe configuration directory at {0}", launchPath);
                }

                if (!Directory.Exists(hashsets_root))
                {
                    Directory.CreateDirectory(hashsets_root);
                    Console.WriteLine("Initalized empty HashSets store directory at {0}", hashsets_root);
                }

                if (!Directory.Exists(temp_root))
                {
                    Directory.CreateDirectory(temp_root);
                    Console.WriteLine("Initalized empty Temp directory at {0}", hashsets_root);
                }
            }
            catch (Exception e)
            {
                LineOutput.WriteLineColor("\nThe root handler encountered an error, {0}", ConsoleColor.Red, e.Message);
                Environment.Exit(1);
            }
        }

        internal static async Task Cmd_Checksum()
        {
            Console.WriteLine(launchPath);
            return;
        }

        internal static async Task Cmd_DownloadHashset(string hashlist_url, string? integrity = null, string? customName = null)
        {
            try{
                if (!hashlist_url.StartsWith("http://") && !hashlist_url.StartsWith("https://")){
                    LineOutput.WriteLineColor("The Hashlist source URL is invalid.", ConsoleColor.Red);
                    return;
                }

                HttpResponseMessage response = await client.GetAsync(hashlist_url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                int totalLength = 0;
                int uploadedNum = 0;
                
                Item? item =  JsonSerializer.Deserialize<Item>(
                    responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (!String.IsNullOrEmpty(responseBody)) {
                    responseBody = responseBody.Substring(0, responseBody.Length - 1);
                } else {
                    throw new Exception("The hashlist on the URL is empty.");
                }
                
                Console.WriteLine();
                LineOutput.LogSuccess("Downloaded {0}", hashlist_url);
                
                if (!String.IsNullOrEmpty(integrity)){
                    string checksum = Hash.sha256(responseBody);
                    integrity = integrity.Trim();
                    Console.WriteLine("\nINCOMING HASH: {0}\nINTEGRITY HASH: {1}", checksum, integrity);

                    if (checksum != integrity){
                        throw new Exception("The supplied integrity hash does not match the checksum of the downloaded file. The web server might be trying to play you dirty!");
                    }
                    else{
                        Console.WriteLine();
                        LineOutput.LogSuccess("Checksum pased");
                    }
                }
                else{
                    LineOutput.LogWarning("An integrity hash was not supplied. It is reccomended to use one.");
                }

                downloader.DeleteTemp();
                Console.WriteLine("Importing ({0}) URLs..", item?.HashList.Count);

                foreach (HashSource source in item.HashList){
                    string source_url = source.sourceUrl;
                    string source_integrity = source.integrity;
                    Console.WriteLine("Importing.. {0}", source_url);

                    HttpResponseMessage sourceDownload = await client.GetAsync(source_url);
                    sourceDownload.EnsureSuccessStatusCode();
                    
                    string rawSource = await sourceDownload.Content.ReadAsStringAsync();
                    string rawSourceHash = Hash.sha256(rawSource);
                    
                    if (rawSourceHash != source_integrity) {
                        LineOutput.WriteLineColor("Failed checksum. Expecting {0} but received {1}.", ConsoleColor.Red, source_integrity, rawSourceHash);
                        throw new Exception("Checksum failed");
                    }
                    else {
                        Console.WriteLine("Checksum passed");
                    }

                    downloader.DownloadTemp(rawSource);
                    totalLength += downloader.NumHashes(rawSource.Length);
                    Console.WriteLine("Downloaded to swap memory");
                }
                
                Console.WriteLine("\nTOTAL # OF HASHES: {0}", totalLength);
                Console.WriteLine("Compiling HashSet, this may take a while..\n");
                string hashsetFileName = Hash.sha256(DateTime.Now.ToString()) + ".dat";

                using (FileStream fs = File.Create(Path.Combine(launchPath, "hashsets", hashsetFileName))){
                    Console.WriteLine("Percentage of Hashes Loaded:");
                    Console.Write("0%");
                    int onePercent = totalLength / 100;
                    MD5Hash hashSet = new MD5Hash(totalLength, fs);
                    hashSet.FillHashes();
                    
                    int currentPercent = 0;
                    foreach (string line in File.ReadLines(Path.Combine(launchPath, "temp", "swapsource.txt")))
                    {
                        hashSet.UploadHash(Encoding.UTF8.GetBytes(line));
                        uploadedNum++;
                        
                        if (uploadedNum == onePercent){
                            Console.Write("\r{0}%", ++currentPercent);
                            uploadedNum = 0;
                        }
                    }
                }
                customName = String.IsNullOrEmpty(customName) ? item.name : customName;
                hashLists.Add(customName, new Downloader.HashList(customName, totalLength, true, hashlist_url, Hash.sha256(responseBody), hashsetFileName));
                
                downloader.UploadJson(hashLists.Values.ToList());
                LineOutput.LogSuccess("Successfully generated hashset {0}", customName);
            }
            catch(Exception e){
                LineOutput.WriteLineColor("\nhashset-get encountered an error", ConsoleColor.Red);
                LineOutput.WriteLineColor(e.Message, ConsoleColor.Red);
            }
            finally{
                Console.WriteLine("Cleaning up..");
                downloader.DeleteTemp();
            }
        }

        internal static async Task Cmd_RemoveHashset(string name)
        {
            Regex regex = new Regex(@"(?i)y(es)?");
            Console.Write("Are you sure that you want to delete the hash set {0}? [Y/n]: ", name);
            string?  response = Console.ReadLine();
    
            if(String.IsNullOrEmpty(response) || !regex.IsMatch(response)) {
                Console.WriteLine("Exiting out of Command...");
                return;
            }
            
            if(hashLists.ContainsKey(name)){
                LineOutput.LogFailure(String.Format("The hash set under the name {0} does not exist.", name));
            } else {
                Downloader.HashList toRemove = hashLists[name];
                string path = Path.Combine(launchPath, "hashsets", toRemove.hashset_source);
                File.Delete(path);
                
                hashLists.Remove(name);
                try {
                    downloader.UploadJson(hashLists.Values.ToList());
                    LineOutput.LogSuccess(String.Format("The hash set {0} has been removed.", name));
                    
                } catch(Exception e) {
                    LineOutput.LogFailure(String.Format("There was an unexpected error when attempting to remove the hashset {0}.", name));
                    LineOutput.LogFailure(String.Format("Error Message: {0}", e.Message));
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
                LineOutput.LogFailure(String.Format("Failed to find a hash set under the name {0}.", name));
                return;
            }
            
            try {
                downloader.UploadJson(hashLists.Values.ToList());
                LineOutput.LogSuccess(String.Format("HashAxe has disabled the hash set {0}.", name));
            } catch(Exception e) {
                LineOutput.LogFailure(String.Format("There was an unexpected error when attempting to disable the hashset {0}.", name));
                LineOutput.LogFailure(String.Format("Error Message: {0}", e.Message));
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
                LineOutput.LogFailure(String.Format("Failed to find a hash set under the name {0}.", name));
            }
            
            try {
                downloader.UploadJson(hashLists.Values.ToList());
                LineOutput.LogSuccess(String.Format("HashAxe has successfully enabled the hash set {0}.", name));
            } catch(Exception e) {
                LineOutput.LogFailure(String.Format("There was an unexpected error when attempting to enable the hashset {0}.", name));
                LineOutput.LogFailure(String.Format("Error Message: {0}", e.Message));
            }
            return;
        }
        
        internal static async Task Cmd_RenameHashSet(string oldName, string newName) {
            if(hashLists.ContainsKey(newName)) {
                LineOutput.LogFailure(String.Format("Exiting Command: There already exists a hash set under the name {0}", newName));
                return;
            }
            else if(hashLists.ContainsKey(oldName)) {
                hashLists[oldName].name = newName;
            } else {
                LineOutput.LogFailure(String.Format("Exiting Command: No Hashlists under the name {0} found.", oldName));
                return;
            }
            
            try {
                downloader.UploadJson(hashLists.Values.ToList());
                LineOutput.LogSuccess(String.Format("The hash set {0} was renamed to {1}.", oldName, newName));
            } catch(Exception e) {
                LineOutput.LogFailure(String.Format("An unexpected error occurred when renaming {0} to {1}.", oldName, newName));
                LineOutput.LogFailure(String.Format("Error Message: {0}", e.Message));
            }
            return;
        }

        internal static async Task Cmd_ListHashSets()
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

        internal static async Task Cmd_Traverse(string searchPath)
        {
            Traverser traverser;
            HashSet<string> flagged = new HashSet<string>();
            
            foreach(Downloader.HashList hashList in hashLists.Values) {
                if(hashList.enabled) {
                    try {
                        using (FileStream fs = File.OpenRead(Path.Combine(launchPath, "hashsets", hashList.hashset_source)))
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
                        LineOutput.LogFailure(String.Format("An Error has stopped the hashList {0} from being properly checked against the files.", hashList.name));
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
        
        private class Item {
            public string? name {get; set;}
            public string? compiledIntegrity {get; set;}
            public List<HashSource>? HashList {get; set;}
        }
        
        private class HashSource {
            public string? sourceUrl {get; set;}
            public string? integrity {get; set;}
        }
    }
}