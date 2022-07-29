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
            Command downloadHashet = new Command("hashset-get", "Install a hashset from a haslist url");
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
                await Cmd_GetHashes(launchPath);
            });

            removeHashet.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_GetHashes(launchPath);
            });

            disableHashset.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_GetHashes(launchPath);
            });

            enableHashset.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_GetHashes(launchPath);
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

        internal static async Task Traverse(string hashaxe_root)
        {
            // there's stiltraverserl an error meep
            Traverser traverser;
            using (FileStream fs = File.Create("data/hashes.dat"))
            {
                MD5Hash hashSet = new MD5Hash(6, fs);
                hashSet.UploadHash("2d75cc1bf8e57872781f9cd04a529256");
                hashSet.UploadHash("00f538c3d410822e241486ca061a57ee");
                hashSet.UploadHash("3f066dd1f1da052248aed5abc4a0c6a1");
                hashSet.UploadHash("781770fda3bd3236d0ab8274577dddde");
                hashSet.UploadHash("86b6c59aa48a69e16d3313d982791398");
                // This is the hash for rawr.dat
                hashSet.UploadHash("ef3a3971679c0368039d909527cdb972");

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
    }
}