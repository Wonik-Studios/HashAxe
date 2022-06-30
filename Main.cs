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
				await Cmd_GetHashes();
			});

			checkSum.SetHandler(async () =>
			{
				await Cmd_Checksum();
			});

			scan.SetHandler(async () =>
			{
				await Cmd_Scan();
			});

			await rCommand.InvokeAsync(args);
		}
        internal static async Task Cmd_GetHashes()
        {
			Console.WriteLine("This command is being written!");
            return;
        }

		internal static async Task Cmd_Checksum()
        {
			Console.WriteLine("This command is being written!");
            return;
        }

		internal static async Task Cmd_Scan()
        {
			Console.WriteLine("This command is being written!");
            return;
        }
    }
}