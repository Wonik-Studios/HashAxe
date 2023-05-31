using System.CommandLine;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using System.Runtime.InteropServices;

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
            Command listHashets = new Command("hashsets", "List all the installed hashsets in the configuration");
            Command compileHashlist = new Command("compile", "Compiles a HashAxe hashlist from a file into a binary .dat file");
            Command removeHashet = new Command("remove", "Uninstalls a hashset from the configuration");
            Command disableHashset = new Command("disable", "Disables a hashset so it won't be included in the scan");
            Command enableHashset = new Command("enable", "Enables a previously disabled hashset");
            Command renameHashset = new Command("rename", "Renames a designated hashset");
            Command installHashset = new Command("install", "Installs a HashSet into the HashAxe configuration");
            Command traverse = new Command("traverse", "Scans the specified path");

            // The root command.
            RootCommand rCommand = new RootCommand("HashAxe is installed, view https://github.com/Wonik-Studios/HashAxe for some example usage.");

            // Set the arguments for the commands.
            Argument<FileSystemInfo> hashlistPathArg = new Argument<FileSystemInfo>("Hashlist Input", "Path to the hashlist json file to be compiled");
            Argument<FileSystemInfo> hashetOutputArg = new Argument<FileSystemInfo>("Hashset Output", "Path to the output hashset file");
            compileHashlist.Add(hashlistPathArg);
            compileHashlist.Add(hashetOutputArg);

            Argument<string> hashsetNameArg = new Argument<string>("HashSet Name", "The name of the hashset to be installed.");
            Argument<FileSystemInfo> datPathArg = new Argument<FileSystemInfo>("DAT Binary Path", "Path to the .dat binary file");
            installHashset.Add(hashsetNameArg);
            installHashset.Add(datPathArg);

            Argument<string> oldNameArg = new Argument<string>("old-name", "The name of the hashset that will be renamed.");
            Argument<string> newNameArg = new Argument<string>("new-name", "The name that the old hashset will be renamed to.");
            renameHashset.Add(oldNameArg);
            renameHashset.Add(newNameArg);

            Argument<string> searchPathArg = new Argument<string>("search-path", "The directory or file that will be traversed and checked for any flagged malware according to the enabled hashsets.");
            traverse.Add(searchPathArg);

            Argument<string> nameEnableArg = new Argument<string>("hashset-name", "The name of the hashlist that will be enabled.");
            enableHashset.Add(nameEnableArg);

            Argument<string> nameDisableArg = new Argument<string>("hashset-name", "The name of the hashlist that will be disabled.");
            disableHashset.Add(nameDisableArg);

            Argument<string> nameRemoveArg = new Argument<string>("hashset-name", "The name of the hashlist to be removed.");
            removeHashet.Add(nameRemoveArg);

            // Add the commands to the root command.
            rCommand.Add(installHashset);
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

            listHashets.SetHandler(() =>
            {
                Cmd_ListHashSets();
            });

            compileHashlist.SetHandler((hashlist_input, hashset_output) =>
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

                Cmd_CompileHashlist(hashlistInput, hashsetOutput);
            }, hashlistPathArg, hashetOutputArg);

            removeHashet.SetHandler((string name) =>
            {
                Cmd_RemoveHashset(name);
            }, nameRemoveArg);

            disableHashset.SetHandler((string name) =>
            {
                Cmd_DisableHashset(name);
            }, nameDisableArg);

            enableHashset.SetHandler((string name) =>
            {
                Cmd_EnableHashset(name);
            }, nameEnableArg);

            renameHashset.SetHandler((string oldName, string newName) =>
            {
                Cmd_RenameHashSet(oldName, newName);
            }, oldNameArg, newNameArg);

            installHashset.SetHandler((string name, FileSystemInfo path) =>
            {
                FileInfo? datPath = null;
                switch (path)
                {
                    case FileInfo file:
                        datPath = file;
                        break;
                    default:
                        Console.WriteLine("DAT Binary Path is invalid");
                        break;
                }
                Cmd_InstallHashset(name, datPath);
            }, hashsetNameArg, datPathArg);

            traverse.SetHandler((string searchPath) =>
            {
                Cmd_Traverse(searchPath);
            }, searchPathArg);

            await rCommand.InvokeAsync(args);
        }

        internal static void root()
        {
            launchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "hashaxe");
            string hashsets_root = Path.Combine(launchPath, "hashsets");
            string temp_root = Path.Combine(launchPath, "temp");
            string config_root = Path.Combine(launchPath, "hashmap.json");

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

                if (!File.Exists(config_root))
                {
                    using (FileStream fs = File.Create(config_root)){
                        fs.Write(Encoding.UTF8.GetBytes("[]"));
                    }
                    Console.WriteLine("Initalized empty HashAxe configuration file at {0}", config_root);
                }
            }
            catch (Exception e)
            {
                LineOutput.WriteLineColor("\nThe root handler encountered an error, {0}", ConsoleColor.Red, e.Message);
                Environment.Exit(1);
            }
        }

        internal static void Cmd_InstallHashset(string name, FileSystemInfo path)
        {
            // Check if name contains any invalid characters.
            int length = 0;
            if (!Regex.IsMatch(name, @"^[a-zA-Z0-9]+$"))
            {
                LineOutput.LogFailure("The name of the hashset contains invalid characters. Please use only alphanumeric characters.");
                return;
            }

            if (!File.Exists(path.FullName))
            {
                LineOutput.LogFailure("The file {0} does not exist or is not a valid .dat", path.FullName);
                return;
            } else {
                using(FileStream fs = File.OpenRead(path.FullName)) {
                    byte[] buffer = new byte[4];
                    fs.Read(buffer, 0, buffer.Length);
                    length = BitConverter.ToInt32(buffer);
                }
            }
            
            if (!path.FullName.EndsWith(".dat"))
            {
                LineOutput.LogFailure("The file {0} is not a valid .dat", path.FullName);
                return;
            }

            if (hashLists.ContainsKey(name))
            {
                LineOutput.LogFailure("HashSet with the same name already exists.");
                return;
            }

            try
            {
                string fileName = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".dat";
                File.Copy(path.FullName, Path.Combine(launchPath, "hashsets", fileName));
                LineOutput.LogSuccess("Copied {0} to {1}", path.FullName, Path.Combine(launchPath, "hashsets", fileName));

                hashLists.Add(name, new Downloader.HashList(name, length, true, fileName));
                downloader.UploadJson(hashLists.Values.ToList());
                LineOutput.LogSuccess("Installed hashset into configuration");
            }
            catch (Exception e)
            {
                Console.WriteLine();
                LineOutput.WriteLineColor("hashset-install encountered an error", ConsoleColor.Red);
                LineOutput.WriteLineColor(e.Message, ConsoleColor.Red);
            }
        }

        internal static void Cmd_CompileHashlist(FileInfo hashlist_input, FileInfo hashset_output)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            Console.WriteLine("Compiling new HashSet from Hashlist");
            Console.WriteLine("Hashlist Input: {0}", hashlist_input.FullName);
            Console.WriteLine("Hashset Output: {0}", hashset_output.FullName);

            try
            {
                downloader.DeleteTemp();
                long length = new System.IO.FileInfo(hashlist_input.FullName).Length;

                int totalLength;
                if(isWindows) {
                    totalLength = (int)((length + 2) / 34);
                } else {
                    totalLength = (int)((length + 2) / 33);
                }
                int uploadedNum = 0;

                Console.WriteLine("Compiling HashSet, this may take a while..\n");
                string hashsetFileName = Hash.sha256(DateTime.Now.ToString()) + ".dat";

                using (BufferedStream fs = new BufferedStream(File.Create(hashset_output.FullName)))
                {
                    Console.WriteLine("Percentage of Hashes Loaded:");
                    Console.Write("0%");

                    MD5Hash hashSet = new MD5Hash(totalLength, fs);
                    hashSet.FillHashes();

                    long prev = 0;
                    using (BufferedStream bs = new BufferedStream(File.OpenRead(hashlist_input.FullName)))
                    {
                        byte[] buffer = new byte[32];
                        for (int i = 0; i < totalLength; i++)
                        {
                            bs.Read(buffer, 0, 32);
                            hashSet.UploadHash(buffer);
                            uploadedNum++;
                            if (isWindows) {
                                bs.Position += 2;
                            } else {
                                bs.Position += 1;
                            }

                            long cur = uploadedNum * 100L / totalLength;
                            if (prev != cur)
                            {
                                Console.Write("\r{0}%", uploadedNum * 100L / totalLength);
                                prev = cur;
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
            catch (Exception e)
            {
                LineOutput.WriteLineColor("\nhashset-compile encountered an error", ConsoleColor.Red);
                LineOutput.WriteLineColor(e.Message, ConsoleColor.Red);
                Console.Write(e.Message);
            }
            finally
            {
                downloader.DeleteTemp();
                Console.WriteLine("\nCleaning up..");
            }
        }

        internal static void Cmd_RemoveHashset(string name)
        {
            Regex regex = new Regex(@"(?i)y(es)?");
            Console.Write("Are you sure that you want to delete the hash set {0}? [Y/n]: ", name);
            string? response = Console.ReadLine();

            if (String.IsNullOrEmpty(response) || !regex.IsMatch(response))
            {
                Console.WriteLine("Exiting out of Command...");
                return;
            }

            if (!hashLists.ContainsKey(name))
            {
                LineOutput.LogFailure(String.Format("The hash set under the name {0} does not exist.", name));
            }
            else
            {
                Downloader.HashList toRemove = hashLists[name];
                string path = Path.Combine(launchPath, "hashsets", toRemove.hashset_source);
                File.Delete(path);

                hashLists.Remove(name);
                try
                {
                    downloader.UploadJson(hashLists.Values.ToList());
                    LineOutput.LogSuccess(String.Format("The hash set {0} has been removed.", name));

                }
                catch (Exception e)
                {
                    LineOutput.LogFailure(String.Format("There was an unexpected error when attempting to remove the hashset {0}.", name));
                    LineOutput.LogFailure(String.Format("Error Message: {0}", e.Message));
                }
            }
            return;
        }

        internal static void Cmd_DisableHashset(string name)
        {
            if (hashLists.ContainsKey(name))
            {
                Downloader.HashList toDisable = hashLists[name];
                toDisable.enabled = false;
            }
            else
            {
                LineOutput.LogFailure(String.Format("Failed to find a hash set under the name {0}.", name));
                return;
            }

            try
            {
                downloader.UploadJson(hashLists.Values.ToList());
                LineOutput.LogSuccess(String.Format("HashAxe has disabled the hash set {0}.", name));
            }
            catch (Exception e)
            {
                LineOutput.LogFailure(String.Format("There was an unexpected error when attempting to disable the hashset {0}.", name));
                LineOutput.LogFailure(String.Format("Error Message: {0}", e.Message));
            }
            return;
        }

        internal static void Cmd_EnableHashset(string name)
        {
            Console.WriteLine("Name: " + name);
            if (hashLists.ContainsKey(name))
            {
                Downloader.HashList toEnable = hashLists[name];
                toEnable.enabled = true;
            }
            else
            {
                LineOutput.LogFailure(String.Format("Failed to find a hash set under the name {0}.", name));
            }

            try
            {
                downloader.UploadJson(hashLists.Values.ToList());
                LineOutput.LogSuccess(String.Format("HashAxe has successfully enabled the hash set {0}.", name));
            }
            catch (Exception e)
            {
                LineOutput.LogFailure(String.Format("There was an unexpected error when attempting to enable the hashset {0}.", name));
                LineOutput.LogFailure(String.Format("Error Message: {0}", e.Message));
            }
            return;
        }

        internal static void Cmd_RenameHashSet(string oldName, string newName)
        {
            if (hashLists.ContainsKey(newName))
            {
                LineOutput.LogFailure(String.Format("Exiting Command: There already exists a hash set under the name {0}", newName));
                return;
            }
            else if (hashLists.ContainsKey(oldName))
            {
                hashLists[oldName].name = newName;
            }
            else
            {
                LineOutput.LogFailure(String.Format("Exiting Command: No Hashlists under the name {0} found.", oldName));
                return;
            }

            try
            {
                downloader.UploadJson(hashLists.Values.ToList());
                LineOutput.LogSuccess(String.Format("The hash set {0} was renamed to {1}.", oldName, newName));
            }
            catch (Exception e)
            {
                LineOutput.LogFailure(String.Format("An unexpected error occurred when renaming {0} to {1}.", oldName, newName));
                LineOutput.LogFailure(String.Format("Error Message: {0}", e.Message));
            }
            return;
        }

        internal static void Cmd_ListHashSets()
        {
            foreach (Downloader.HashList entry in hashLists.Values)
            {
                Console.WriteLine("-------------------------------------------------------------------------");
                Console.WriteLine("NAME             | {0}", entry.name);
                Console.WriteLine("TIME OF CREATION | {0}", entry.hashset_source.Substring(0, entry.hashset_source.Length - 4));
                Console.WriteLine("# OF HASHES      | {0}", entry.NUM_HASHES);
                Console.WriteLine("ENABLED          | {0}", entry.enabled ? "YES" : "NO");
            }
            Console.WriteLine("-------------------------------------------------------------------------");
            return;
        }

        internal static void Cmd_Traverse(string searchPath)
        {
            Traverser traverser;
            HashSet<string> flagged = new HashSet<string>();

            foreach (Downloader.HashList hashList in hashLists.Values)
            {
                if (hashList.enabled)
                {
                    try
                    {
                        using (FileStream fs = File.OpenRead(Path.Combine(launchPath, "hashsets", hashList.hashset_source)))
                        {
                            MD5Hash hashSet = new MD5Hash(fs);
                            using (MD5 md5 = MD5.Create())
                            {
                                traverser = new Traverser(searchPath, hashSet, md5);
                                traverser.Traverse();
                            }
                        }
                        flagged.UnionWith(traverser.GetFlagged());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(String.Format("An error has stopped the hashList \"{0}\" from being properly checked against the files.", hashList.name));
                        LineOutput.LogFailure(e.Message);
                        
                    }
                }
            }

            Console.WriteLine(String.Format("\n\n{0} HITS", flagged.Count));
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (string file in flagged)
            {
                Console.WriteLine(file);
            }
            Console.ForegroundColor = ConsoleColor.White;

            
            if(flagged.Count > 0) {
                Console.WriteLine();
                Console.Write("Do you want to delete these files? [Y/n]: ");
                
                Regex regex = new Regex(@"(?i)y(es)?");
                string? response = Console.ReadLine();

                if (response != null && regex.IsMatch(response))
                {
                    int success = 0;
                    foreach(string file in flagged) {
                        try {
                            File.Delete(file);
                            success++;
                        } catch(Exception e) {
                            Console.WriteLine("Failed to Remove the file {0}", file);
                        }
                    }
                    LineOutput.LogSuccess("Successfully deleted {0} files.", success);
                }
            }
            Console.WriteLine("Exiting out of Command...");
            return;
        }
    }
}