using System.CommandLine;

#pragma warning disable CS8600
#pragma warning disable CS8604

namespace HashAxe
{
    class HashAxeMain
    {
        static async Task Main(string[] args)
        {
            Command getHashes = new Command("get-hashes", "Download the latest set of hashes.");
            Command checkSum = new Command("checksum", "Verifies the local hashes that no alertations have occured.");
            Command scan = new Command("scan", "Runs the HashAxe scan on a path.");
            RootCommand rCommand = new RootCommand("This is the root command for HashAxe made by Wonik. If you are seeing this, that means HashAxe is installed properly.");

            rCommand.Add(getHashes);
            rCommand.Add(checkSum);
            rCommand.Add(scan);

            getHashes.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_GetHashes(launchPath);
            });

            checkSum.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_Checksum(launchPath);
            });

            scan.SetHandler(async () =>
            {
                string launchPath = root();
                await Cmd_Scan(launchPath);
            });

            await rCommand.InvokeAsync(args);
        }

        internal static string root()
        {
            Console.WriteLine(@"
██╗░░██╗░█████╗░░██████╗██╗░░██╗░█████╗░██╗░░██╗███████╗
██║░░██║██╔══██╗██╔════╝██║░░██║██╔══██╗╚██╗██╔╝██╔════╝
███████║███████║╚█████╗░███████║███████║░╚███╔╝░█████╗░░
██╔══██║██╔══██║░╚═══██╗██╔══██║██╔══██║░██╔██╗░██╔══╝░░
██║░░██║██║░░██║██████╔╝██║░░██║██║░░██║██╔╝╚██╗███████╗
╚═╝░░╚═╝╚═╝░░╚═╝╚═════╝░╚═╝░░╚═╝╚═╝░░╚═╝╚═╝░░╚═╝╚══════╝");
			Console.WriteLine("HasAxe {version} by Wonik");
            return "I like to move it move it";
        }
        internal static async Task Cmd_GetHashes(string hashaxe_root)
        {
            Console.WriteLine(hashaxe_root);
            return;
        }

        internal static async Task Cmd_Checksum(string hashaxe_root)
        {
            Console.WriteLine(hashaxe_root);
            return;
        }

        internal static async Task Cmd_Scan(string hashaxe_root)
        {
            Console.WriteLine(hashaxe_root);
            return;
        }
    }
}