using DiscordBotApiWrapper;
using DiscordBotApiWrapper.Dtos;
using DiscordBotApiWrapper.Models;
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

            //var test = await serverLogItem.GetServerLogItem(1);
            //var test = await serverLogItem.GetServerLogIdsForGuild(123, 1);

            var responseCode = await serverLogItem.SaveServerLogItem(new ServerLogItemCreateDto
            {
                Guild = new GuildCreateDto { GuildId = 123, GuildName = "Cool Guild" },
                Channel = new ChannelCreateDto { ChannelId = 321, ChannelName = "Logging Channel"},
                Title = "Wrapper Test",
                Description = "Just testing the wrapper",
                ThumbnailUrl = "https://example.com/test/jpg"
            });

            var test = await serverLogItem.GetServerLogIdsForGuild(123);

            //Console.WriteLine(test.Description);

            foreach (var t in test)
            {
                Console.WriteLine(t.Description);
            }
        }
    }
}
