using System.CommandLine;

namespace HashAxe.Main
{
    class MainClass
    {
        static async Task Main(string[] args)
        {
            Argument<string> testArgument = new Argument<string>(
                name: "test",
                description: "test test test");
            RootCommand rCommand = new RootCommand("Test");
            rCommand.Add(testArgument);
            rCommand.SetHandler((test) =>
            {
                Command(test);
            }, testArgument);
            await rCommand.InvokeAsync(args);
        }
        static void Command(string test)
        {
            Console.WriteLine($"<- {test}");
        }
    }
}