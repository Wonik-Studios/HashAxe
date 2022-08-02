using System.CommandLine;
using System.Reflection;
using ModifiedOutput;
using HashAxe.FileTraverser;
using HashAxe.MD5HashSet;
using System.Security.Cryptography;


#pragma warning disable CS8600
#pragma warning disable CS8604

namespace HashAxe
{
    class HashAxeMain
    {

        static async Task Main(string[] args)
        {
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

            string launchPath = root();

            checksum.SetHandler(async () =>
            {
                await Cmd_Checksum(launchPath);
            });

            listHashets.SetHandler(async () =>
            {
                await Cmd_ListHashes(launchPath);
            });

            downloadHashet.SetHandler(async () =>
            {
                await Cmd_DownloadHashset(launchPath);
            });

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

        internal static async Task Cmd_DownloadHashset(string hashaxe_root)
        {
            Console.WriteLine(hashaxe_root);
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

        internal static async Task Cmd_Traverse(string hashaxe_root)
        {
            // there's stiltraverserl an error meep
            Traverser traverser;
            using (FileStream fs = File.OpenRead(Path.Combine(hashaxe_root, "hashsets", "ashdahsdhasd")))
            {
                MD5Hash hashSet = new MD5Hash(6, fs);
                using (MD5 md5 = MD5.Create())
                {
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
            foreach (string file in traverser.GetFlagged())
            {
                Console.WriteLine(file);
            }
        }

        private class Item {
            public string name;
            public int NUM_HASHES;
        }

        private static Dictionary<string, Item> initialize() {

        }
    }
}
