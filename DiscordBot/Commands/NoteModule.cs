/*
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
using DiscordBotLib.DataAccess;
using DiscordBotLib.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    [Name("Note")]
    [Group("note")]
    public class NoteModule : ModuleBase<SocketCommandContext>
    {
        private readonly INoteRepository _noteRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<NoteModule> _logger;

        public NoteModule(INoteRepository noteRepository,
            IUserRepository userRepository,
            ILogger<NoteModule> logger)
        {
            _noteRepository = noteRepository;
            _userRepository = userRepository;
            _logger = logger;
        }


        [Command("create")]
        [Summary("Create a note")]
        public async Task NoteCreate([Summary("Note name")]string name, [Summary("Note Text")][Remainder] string text)
        {
            await ReplyAsync("Command Disabled for now....");
            return;

            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed note create (Name: {name} Text: {text}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, name, text, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            User user = await _userRepository.GetByUserId(Context.User.Id);
            if(user == null)
            {
                user = new User { UserId = Context.User.Id, UserName = Context.User.Username };
                await _userRepository.AddAsync(user);
            }

            if(user.UserName != Context.User.Username)
            {
                user.UserName = Context.User.Username;
                await _userRepository.EditAsync(user);
            }

            Note note = new Note { Name = name, Text = text };
            await _noteRepository.AddAsync(note, user);

            await ReplyAsync ($"Note `{name}` create!");
        }
    }
}
