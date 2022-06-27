using System.CommandLine;

#pragma warning disable CS8600
#pragma warning disable CS8604

namespace HashAxe
{
    class HashAxeMain
    {
		static async Task Main(string[] args)
		{
			Argument<string> password = new Argument<string>(
				name: "password",
				description: "Password used to validate your status as an administrator");
			RootCommand rCommand = new RootCommand("C# client to assist with retrieving Wonik API keys.");
			rCommand.Add(password);
			rCommand.SetHandler(async (password) =>
			{
				await Command(password);
			},
			password);
			await rCommand.InvokeAsync(args);
		}
        internal static async Task Command(string testArgument)
        {
            Console.WriteLine(@"
            
██╗░░██╗░█████╗░░██████╗██╗░░██╗░█████╗░██╗░░██╗███████╗
██║░░██║██╔══██╗██╔════╝██║░░██║██╔══██╗╚██╗██╔╝██╔════╝
███████║███████║╚█████╗░███████║███████║░╚███╔╝░█████╗░░
██╔══██║██╔══██║░╚═══██╗██╔══██║██╔══██║░██╔██╗░██╔══╝░░
██║░░██║██║░░██║██████╔╝██║░░██║██║░░██║██╔╝╚██╗███████╗
╚═╝░░╚═╝╚═╝░░╚═╝╚═════╝░╚═╝░░╚═╝╚═╝░░╚═╝╚═╝░░╚═╝╚══════╝
            ");
        }
    }
}