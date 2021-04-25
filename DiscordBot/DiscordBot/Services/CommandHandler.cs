﻿/*
MIT License

Copyright(c) 2021 Kyle Givler
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

using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        private readonly string _prefix;

        public CommandHandler(DiscordSocketClient client,
            CommandService commands,
            IConfiguration config,
            IServiceProvider serviceProvider)
        {
            _client = client;
            _commands = commands;
            _config = config;
            _serviceProvider = serviceProvider;

            _prefix = _config.GetSection("Prefix").Value;

            _client.MessageReceived += MessageReceived;
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            if(message.Author.IsBot)
            {
                return;
            }

            var context = new SocketCommandContext(_client, message);

            int position = 0;

            if(message.HasStringPrefix(_prefix, ref position) || message.HasMentionPrefix(_client.CurrentUser, ref position))
            {
                var result = await _commands.ExecuteAsync(context, position, _serviceProvider);

                if(!result.IsSuccess)
                {
                    Console.WriteLine($"The following error occured: \n{result.Error}");
                }
            }
        }
    }
}