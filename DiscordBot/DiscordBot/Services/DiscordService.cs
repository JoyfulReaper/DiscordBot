/*
MIT License

Copyright(c) 2020 Kyle Givler
https://github.com/JoyfulReaper

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class DiscordService : IChatService
    {
        private readonly IConfiguration _configuration;

        private readonly DiscordSocketClient _socketClient = new DiscordSocketClient();

        public DiscordService(IConfiguration configuration)
        {
            _configuration = configuration;

            _socketClient.Ready += SocketClient_Ready;
            _socketClient.MessageReceived += SocketClient_MessageReceived;
            _socketClient.Disconnected += SocketClient_Disconnected;
            _socketClient.Log += SocketClient_Log;
        }

        private Task SocketClient_MessageReceived(SocketMessage arg)
        {
            Console.WriteLine("Message received: ");
            Console.WriteLine($"{arg.Author.Username} : {arg.Channel.Name} : {arg.Content}");

            return Task.CompletedTask;
        }

        private Task SocketClient_Disconnected(Exception arg)
        {
            Console.WriteLine("SocketClient disconnected!");
            Environment.Exit(1);

            return Task.CompletedTask;
        }

        private Task SocketClient_Ready()
        {
            Console.WriteLine("SocketClient is ready");
            return Task.CompletedTask;
        }

        private Task SocketClient_Log(LogMessage arg)
        {
            Console.WriteLine(arg.Message);
            return Task.CompletedTask;
        }

        public async Task Start()
        {
            var token = File.ReadAllText(@"C:\token.txt");
            await _socketClient.LoginAsync(TokenType.Bot, token);
            await _socketClient.StartAsync();
        }
    }
}
