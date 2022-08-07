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
            Command compileHashlist = new Command("compile", "Compiles a HashAxe hashlist from a file into a binary .dat file");
            Command removeHashet = new Command("hashset-remove", "Uninstalls a hashset from the configuration");
            Command disableHashset = new Command("hashset-off", "Disables a hashset so it won't be included in the scan");
            Command enableHashset = new Command("hashset-on", "Enables a previously disabled hashset");
            Command renameHashset = new Command("rename", "Renames a designated hashset");
            Command traverse = new Command("traverse", "Scans the specified path");

            // The root command.
            RootCommand rCommand = new RootCommand("This is the root command for HashAxe made by Wonik. If you are seeing this, that means HashAxe is installed properly.");

            // Set the arguments for the commands.
            Argument<FileSystemInfo> hashlistPathArg = new Argument<FileSystemInfo>("Hashlist Input", "Path to the hashlist json file to be compiled");
            Argument<FileSystemInfo> hashetOutputArg = new Argument<FileSystemInfo>("Hashset Output", "Path to the output hashset file");
            compileHashlist.Add(hashlistPathArg);
            compileHashlist.Add(hashetOutputArg);
            
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
            rCommand.Add(compileHashlist);
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

            compileHashlist.SetHandler(async ( hashlist_input, hashset_output) =>
            {
                FileInfo? hashlistInput = null;
                FileInfo? hashsetOutput = null;

                switch (hashlist_input)
                {
                    case FileInfo file:
                        hashlistInput = file;
                        break;
                    default:
                        Console.WriteLine("Hashlist input argument is invalid");
                        break;
                }

                switch (hashset_output)
                {
                    case FileInfo file:
                        hashsetOutput = file;
                        break;
                    default:
                        Console.WriteLine("Hashset output is invalid");
                        break;
                }

                await Cmd_CompileHashlist(hashlistInput, hashsetOutput);
            }, hashlistPathArg, hashetOutputArg);

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

        internal static async Task Cmd_CompileHashlist(FileInfo hashlist_input, FileInfo hashset_output)
        {
            Console.WriteLine("Compiling new HashSet from Hashlist");
            Console.WriteLine("Hashlist Input: {0}", hashlist_input.FullName);
            Console.WriteLine("Hashset Output: {0}\n", hashset_output.FullName);

            try{
                
                string responseBody = File.ReadAllText(hashlist_input.FullName);
                
                Item? item =  JsonSerializer.Deserialize<Item>(
                    responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (!String.IsNullOrEmpty(responseBody)) {
                    responseBody = responseBody.Substring(0, responseBody.Length - 1);
                } else {
                    throw new Exception("The hashlist on the URL is empty.");
                }

                int totalLength = 0;
                int uploadedNum = 0;

                downloader.DeleteTemp();

                if (item.HashList != null) {
                    Console.WriteLine("Importing ({0}) URLs..", item?.HashList.Count);
                    foreach (HashSource source in item.HashList)
                    {
                        string source_url = source.sourceUrl;
                        string source_integrity = source.integrity;
                        Console.WriteLine("Importing.. {0}", source_url);

                        HttpResponseMessage sourceDownload = await client.GetAsync(source_url);
                        sourceDownload.EnsureSuccessStatusCode();

                        string rawSource = await sourceDownload.Content.ReadAsStringAsync();
                        string rawSourceHash = Hash.sha256(rawSource);

                        if (rawSourceHash != source_integrity)
                        {
                            LineOutput.WriteLineColor("Failed checksum. Expecting {0} but received {1}.", ConsoleColor.Red, source_integrity, rawSourceHash);
                            throw new Exception("Checksum failed");
                        }
                        else
                        {
                            Console.WriteLine("Checksum passed");
                        }

                        downloader.DownloadTemp(rawSource);
                        totalLength += downloader.NumHashes(rawSource.Length);
                        Console.WriteLine("Downloaded to swap memory");
                    }
                }
                
                int numRawHashes = item.rawHashes != null ? item.rawHashes.Count : 0;
                totalLength += numRawHashes;
                        
                Console.WriteLine("\nTOTAL # OF HASHES: {0}", totalLength);
                Console.WriteLine("Compiling HashSet, this may take a while..\n");
                string hashsetFileName = Hash.sha256(DateTime.Now.ToString()) + ".dat";

                using (BufferedStream fs = new BufferedStream(File.Create(hashset_output.FullName))) {
                    Console.WriteLine("Percentage of Hashes Loaded:");
                    Console.Write("0%");
                    
                    MD5Hash hashSet = new MD5Hash(totalLength, fs);
                    hashSet.FillHashes();
                    
                    long prev = 0;
                    if (item.rawHashes != null){
                        foreach (string rawHash in item.rawHashes)
                        {
                            hashSet.UploadHash(Encoding.UTF8.GetBytes(rawHash));
                            uploadedNum++;

                            long cur = uploadedNum * 100L / totalLength;
                            if(prev != cur) {
                                Console.Write("\r{0}%", uploadedNum * 100L / totalLength);
                                prev = cur;
                            }
                        }
                    }
                    
                    if(item.HashList != null) {
                        using(BufferedStream bs = new BufferedStream(File.OpenRead(Path.Combine(launchPath, "temp", "swapsource.txt")))) {
                            byte[] buffer = new byte[32];
                            for(int i=0; i < totalLength - numRawHashes; i++) {
                                bs.Read(buffer, 0, 32);
                                hashSet.UploadHash(buffer);
                                uploadedNum++;
                                bs.Position++;
                                
                                long cur = uploadedNum * 100L / totalLength;
                                if(prev != cur) {
                                    Console.Write("\r{0}%", uploadedNum * 100L / totalLength);
                                    prev = cur;
                                }
                            }
                        }
                    }
                }
                
                
                Console.WriteLine();
                Console.WriteLine();
                LineOutput.LogSuccess("HashSet successfully compiled:");
                Console.WriteLine("# OF HASHES: {0}", totalLength);
                Console.WriteLine("OUTPUT: {0}", hashset_output.FullName);
            }
            catch(Exception e){
                LineOutput.WriteLineColor("\nhashset-get encountered an error", ConsoleColor.Red);
                LineOutput.WriteLineColor(e.Message, ConsoleColor.Red);
                Console.Write(e.StackTrace);
            }
            finally{
                downloader.DeleteTemp();
                Console.WriteLine("\nCleaning up..");
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
            public List<HashSource>? HashList {get; set;}
            public List<string>? rawHashes {get; set;}
        }
        
        private class HashSource {
            public string? sourceUrl {get; set;}
            public string? integrity {get; set;}
        }
    }
}