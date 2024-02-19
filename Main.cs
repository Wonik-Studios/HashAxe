#pragma warning disable CS8618
#pragma warning disable CS8604

using System.CommandLine;
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
            Command cmdList = new Command("ls", "List hashsets.");
            Command cmdCompile = new Command("compile", "Compile lists of hashes to hashset.");
            Command cmdRemove = new Command("rm", "Uninstalls a hashset from the configuration.");
            Command cmdDisable = new Command("off", "Disables a hashset so it won't be included in the scan.");
            Command cmdEnable = new Command("on", "Enables a previously disabled hashset.");
            Command cmdInstall = new Command("i", "Install a hashset.");
            Command cmdScan = new Command("scan", "Scan a specified path.");

            // The root command.
            RootCommand rCommand = new RootCommand("HashAxe is installed, view https://github.com/Wonik-Studios/HashAxe for some example usage.");

            /* Options */
            Option<bool> isVerbose = new Option<bool>(name: "-V", description: "Enables rich logging.", getDefaultValue: () => false);
            cmdScan.Add(isVerbose);

            /* Arguments */

            Argument<FileSystemInfo> hashlistPathArg = new Argument<FileSystemInfo>("input", "Input hashlist file.");
            cmdCompile.Add(hashlistPathArg);

            Argument<FileSystemInfo> datPathArg = new Argument<FileSystemInfo>("path", "Path to hashset.");
            cmdInstall.Add(datPathArg);

            Argument<string> searchPathArg = new Argument<string>("target", "Target path to scan.");
            cmdScan.Add(searchPathArg);

            Argument<string> nameArg = new Argument<string>("name", "Hashset name.");
            cmdEnable.Add(nameArg);
            cmdDisable.Add(nameArg);
            cmdRemove.Add(nameArg);

            // Add the commands to the root command.
            rCommand.Add(cmdInstall);
            rCommand.Add(cmdList);
            rCommand.Add(cmdCompile);
            rCommand.Add(cmdRemove);
            rCommand.Add(cmdDisable);
            rCommand.Add(cmdEnable);
            rCommand.Add(cmdScan);

            // Initialize the launchPath directory as "launchPath", the hashLists which will be responsible for holding data about the hashsets and the downloader.
            root();
            downloader = new Downloader(launchPath);
            hashLists = downloader.GetHashLists();

            cmdList.SetHandler(ListHashSets);

            cmdCompile.SetHandler((hashlist_input) =>
            {
                FileInfo? hashlistInput = null;

                switch (hashlist_input)
                {
                    case FileInfo file:
                        hashlistInput = file;
                        break;
                    default:
                        Console.WriteLine("Hashlist input argument is invalid");
                        break;
                }

                CompileHashlist(hashlistInput);
            }, hashlistPathArg);

            cmdRemove.SetHandler((string name) =>
            {
                RemoveHashset(name);
            }, nameArg);

            cmdDisable.SetHandler((string name) =>
            {
                DisableHashset(name);
            }, nameArg);

            cmdEnable.SetHandler((string name) =>
            {
                EnableHashset(name);
            }, nameArg);

            cmdInstall.SetHandler((FileSystemInfo path) =>
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
                InstallHashset(datPath);
            }, datPathArg);

            cmdScan.SetHandler((string searchPath, bool verboseLogging) =>
            {
                Traverse(searchPath, verboseLogging);
            }, searchPathArg, isVerbose);

            await rCommand.InvokeAsync(args);
        }

        internal static void root()
        {
            launchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".hashaxe");
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
                    using (FileStream fs = File.Create(config_root))
                    {
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

        internal static void InstallHashset(FileSystemInfo path)
        {
            // Check if name contains any invalid characters.
            int length = 0;

            if (!File.Exists(path.FullName))
            {
                LineOutput.LogFailure("Could not find {0}", path.FullName);
                return;
            }
            else
            {
                using (FileStream fs = File.OpenRead(path.FullName))
                {
                    byte[] buffer = new byte[4];
                    fs.Read(buffer, 0, buffer.Length);
                    length = BitConverter.ToInt32(buffer);
                }
            }

            if (!path.FullName.EndsWith(".dat"))
            {
                LineOutput.LogFailure("Not a valid Hashset.");
                return;
            }

            string name = path.Name;
            int next = 1;

            while (hashLists.ContainsKey(name)) {
                name = $"{path.Name}_{next}";
                next++;
            }

            try
            {
                string fileName = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".dat";
                File.Copy(path.FullName, Path.Combine(launchPath, "hashsets", fileName));
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

        internal static void CompileHashlist(FileInfo input)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            Console.WriteLine("Compiling new HashSet from Hashlist");
            Console.WriteLine("Hashlist Input: {0}", input.FullName);

            try
            {
                downloader.DeleteTemp();
                long length = new System.IO.FileInfo(input.FullName).Length;

                int totalLength;
                int positionAdd;
                int uploadedNum = 0;

                if (isWindows)
                {
                    totalLength = (int)((length + 2) / 34);
                    positionAdd = 2;
                }
                else
                {
                    totalLength = (int)((length + 2) / 33);
                    positionAdd = 1;
                }
                
                Console.WriteLine("Compiling HashSet, this may take a while..\n");

                using (BufferedStream fs = new BufferedStream(File.Create($"{input.Name}.dat")))
                {
                    Console.WriteLine("Percentage of Hashes Loaded:");
                    Console.Write("0%");

                    MD5Hash hashSet = new MD5Hash(totalLength, fs);
                    hashSet.FillHashes();

                    long prev = 0;
                    using (BufferedStream bs = new BufferedStream(File.OpenRead(input.FullName)))
                    {
                        byte[] buffer = new byte[32];
                        for (int i = 0; i < totalLength; i++)
                        {
                            bs.Read(buffer, 0, 32);
                            buffer = Encoding.ASCII.GetBytes(Encoding.ASCII.GetString(buffer).ToLower());
                            hashSet.UploadHash(buffer);
                            uploadedNum++;
                            bs.Position += positionAdd;

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
            }
            catch (Exception e)
            {
                LineOutput.WriteLineColor("\nhashset-compile encountered an error", ConsoleColor.Red);
                LineOutput.WriteLineColor(e.Message, ConsoleColor.Red);
                Console.Write(e.Message);
            }
            finally
            {
                Console.WriteLine("\nCleaning up..");
                downloader.DeleteTemp();
            }
        }

        internal static void RemoveHashset(string name)
        {
            if (!hashLists.ContainsKey(name))
            {
                LineOutput.LogFailure("Hashset does not exist.");
                return;
            }

            Regex regex = new Regex(@"(?i)y(es)?");
            Console.Write("Are you sure that you want to delete the hashset {0}? [Y/n]: ", name);
            string? response = Console.ReadLine();

            if (String.IsNullOrEmpty(response) || !regex.IsMatch(response))
            {
                Console.WriteLine("Exiting out of Command...");
                return;
            }

            Downloader.HashList toRemove = hashLists[name];
            string path = Path.Combine(launchPath, "hashsets", toRemove.hashset_source);
            File.Delete(path);

            hashLists.Remove(name);
            try
            {
                downloader.UploadJson(hashLists.Values.ToList());
                LineOutput.LogSuccess(String.Format("{0} Removed.", name));

            }
            catch (Exception e)
            {
                LineOutput.LogFailure(String.Format("Unexpected error removing hashset {0}.", name));
                LineOutput.LogFailure(String.Format("Error Message: {0}", e.Message));
            }
        }

        internal static void DisableHashset(string name)
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
                LineOutput.LogSuccess(String.Format("{0} Disabled.", name));
            }
            catch (Exception e)
            {
                LineOutput.LogFailure(String.Format("Unexpected error disabling {0}.", name));
                LineOutput.LogFailure(String.Format("Error Message: {0}", e.Message));
            }
            return;
        }

        internal static void EnableHashset(string name)
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

        internal static void ListHashSets()
        {
            foreach (Downloader.HashList entry in hashLists.Values)
            {
                Console.WriteLine("-------------------------------------------------------------------------");
                Console.WriteLine("NAME             | {0}", entry.name);
                Console.WriteLine("# OF HASHES      | {0}", entry.NUM_HASHES);
                Console.WriteLine("ENABLED          | {0}", entry.enabled ? "YES" : "NO");
            }

            if (hashLists.Values.Count > 0) {
                Console.WriteLine("-------------------------------------------------------------------------");
            }

            return;
        }

        internal static void Traverse(string searchPath, bool verboseLogging)
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
                                traverser = new Traverser(searchPath, hashSet, md5, verboseLogging);
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


            if (flagged.Count > 0)
            {
                Console.WriteLine();
                Console.Write("Do you want to delete these files? [Y/n]: ");

                Regex regex = new Regex(@"(?i)y(es)?");
                string? response = Console.ReadLine();

                if (response != null && regex.IsMatch(response))
                {
                    int success = 0;
                    foreach (string file in flagged)
                    {
                        try
                        {
                            File.Delete(file);
                            success++;
                        }
                        catch (Exception)
                        {
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