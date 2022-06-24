using HashAxe.ExampleHttp;

namespace HashAxe.Main
{
    class MainClass
    {
        static async Task Main(string[] args)
        {
            await ExampleRequest.PerformRequest();
        }
    }
}