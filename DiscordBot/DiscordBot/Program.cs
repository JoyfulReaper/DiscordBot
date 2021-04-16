using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        private static DiscordSocketClient _client;

        public static async Task Main(string[] args)
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;

            //var token = "tokenHere";
            var token = File.ReadAllText(@"C:\token.txt");

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
