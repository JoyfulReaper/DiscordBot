﻿/*
MIT License

Copyright(c) 2022 Kyle Givler
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
using DiscordBotLibrary.ConfigSections;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Services;

public class CommandHandler : ICommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly BotInformation _botInfo;

    public CommandHandler(DiscordSocketClient client,
        CommandService commands,
        IServiceProvider services,
        IConfiguration config)
    {
        _client = client;
        _commands = commands;
        _services = services;
        _config = config;

        _botInfo = config.GetRequiredSection("BotInformation") as BotInformation ?? new BotInformation();
    }

    public async Task InstallCommandsAsync()
    {
        _client.MessageReceived += HandleCommandAsync;

        await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
            services: _services);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        int argPos = 0;

        if (!(message.HasStringPrefix(_botInfo.DefaultPrefix, ref argPos) ||
            message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
            return;

        var context = new SocketCommandContext(_client, message);

        await _commands.ExecuteAsync(
            context: context,
            argPos: argPos,
            services: _services);
    }
}
