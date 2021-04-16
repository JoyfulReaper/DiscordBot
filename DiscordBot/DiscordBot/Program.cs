using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        private DiscordSocketClient _client;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _client.MessageReceived += CommandHandler;
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

        private Task CommandHandler(SocketMessage message)
        {
            Console.WriteLine($"{message.Author} : {message.Channel} : {message.Content}");

            string command = String.Empty;
            
            if(!message.Content.StartsWith('~') || message.Author.IsBot)
            {
                return Task.CompletedTask;
            }

            int lengthOfCommand = -1;

            if (message.Content.Contains(' '))
            {
                lengthOfCommand = message.Content.IndexOf(' ');
            }
            else
            {
                lengthOfCommand = message.Content.Length;
            }

            string trailing;
            var split = message.Content.Split(' ');
            StringBuilder sb = new StringBuilder();
            for(int i = 1; i < split.Length; i++)
            {
                sb.Append($"{split[i]} ");
            }

            command = message.Content.Substring(1, lengthOfCommand - 1);

            Console.WriteLine($"Command: {command} sb: {sb.ToString()}");
            message.Channel.SendMessageAsync($"Command: {command} sb: {sb.ToString()}");

            if(command.Equals("echo"))
            {
                message.Channel.SendMessageAsync(sb.ToString());
            }

            return Task.CompletedTask;
        }
    }
}
