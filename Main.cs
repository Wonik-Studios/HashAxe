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
			RootCommand rCommand = new RootCommand("\"HashAxe\" -- Nathan");
			rCommand.Add(getHashes);
			getHashes.SetHandler(async () =>
			{
				await GetHashes();
			});
			await rCommand.InvokeAsync(args);
		}
        internal static async Task GetHashes()
        {
            return;
        }
    }
}