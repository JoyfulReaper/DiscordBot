using DiscordBotApiWrapper;
using System;
using System.Threading.Tasks;

namespace ApiWrapperTestDemoThing
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ApiClient client = new ApiClient("JoyfulReaper", "DiscordBot123");
            client.BaseAddress = new Uri("https://localhost:5001");
            ServerLogItemApi serverLogItem = new ServerLogItemApi(client);

            var test = await serverLogItem.GetServerLogItem(1);

            Console.WriteLine(test.Description);
        }
    }
}
