using System.Net.Http;

namespace HashAxe.ExampleHttp
{
    public static class ExampleRequest
    {
        private static HttpClient client = new HttpClient();
        public static async Task<int> PerformRequest()
        {
            try
            {
                HttpRequestMessage req = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://wonik.tech"),
                };
                var res = await client.SendAsync(req).ConfigureAwait(false);
                string resBody = await res.Content.ReadAsStringAsync();
                Console.WriteLine(resBody);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}