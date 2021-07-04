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
using DiscordBotLib.Helpers;
using DiscordBotLib.Models;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    [Name("Note")]
    [Group("note")]
    public class NoteModule : ModuleBase<SocketCommandContext>
    {
        private readonly INoteRepository _noteRepository;
        private readonly ILogger<NoteModule> _logger;
        private readonly IUserService _userService;
        private readonly ISettings _settings;
        private readonly IServerService _serverService;

        public NoteModule(INoteRepository noteRepository,
            ILogger<NoteModule> logger,
            IUserService userService,
            ISettings settings,
            IServerService serverService)
        {
            _noteRepository = noteRepository;
            _logger = logger;
            _userService = userService;
            _settings = settings;
            _serverService = serverService;
        }


        [Command("create")]
        [Summary("Create a note")]
        [Alias("add")]
        public async Task NoteCreate([Summary("Note name")]string name = null, [Summary("Note Text")][Remainder] string text = null)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed note create (Name: {name} Text: {text}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, name, text, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if(text == null || name == null)
            {
                var server = await _serverService.GetServer(Context.Guild);
                await ReplyAsync($"Usage: {server.Prefix}note create {{name}} {{note text}}");
                return;
            }

            var user = await _userService.GetUser(Context.User);
            var notes = await _noteRepository.GetNotesByUserId(user.UserId);

            if ( notes.Count() >= _settings.MaxUserNotes)
            {
                await ReplyAsync($"You have reached the maximum number of notes (`{_settings.MaxUserNotes}`). Please delete one first!");
                return;
            }

            if(notes.Where(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant()).Any())
            {
                await ReplyAsync($"You already have a note named `{name}` please pick a different name.");
                return;
            }

            Note note = new Note { Name = name, Text = text, UserId = user.Id };
            await _noteRepository.AddAsync(note);

            await ReplyAsync ($"Note `{name}` created!");
        }

        [Command("list")]
        [Alias("show")]
        [Summary("list user notes")]
        public async Task NoteList()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed note list on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var user = await _userService.GetUser(Context.User);
            var notes = (await _noteRepository.GetNotesByUserId(user.UserId))?.ToList();

            if(notes == null || !notes.Any())
            {
                await ReplyAsync("You haven't created any notes yet!");
                return;
            }

            string output = string.Empty;
            for(int i = 0; i < notes.Count(); i++)
            {
                output += i + 1 + ") " + notes[i].Name + "\n";
            }

            await ReplyAsync(output);
        }

        [Command("delete")]
        [Alias("remove")]
        [Summary("delete user notes")]
        public async Task NoteDelete([Summary("The name of the note to delete")]string name)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed note delete ({name}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, name, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var user = await _userService.GetUser(Context.User);
            var notes = (await _noteRepository.GetNotesByUserId(user.UserId))?.ToList();
            var noteToDelete = notes?.Where(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant()).SingleOrDefault();

            if(noteToDelete == null)
            {
                await ReplyAsync($"Note `{name}` could not be found!");
                return;
            }

            await _noteRepository.DeleteAsync(noteToDelete);
            await ReplyAsync($"Deleted `{name}`");
        }

        [Command("show")]
        [Alias("read")]
        [Summary("show user note")]
        public async Task NoteShow([Summary("The name of the note to show")][Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed note show ({name}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, name, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var user = await _userService.GetUser(Context.User);
            var notes = (await _noteRepository.GetNotesByUserId(user.UserId))?.ToList();
            var noteToShow = notes?.Where(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant()).SingleOrDefault();

            if (noteToShow == null)
            {
                await ReplyAsync($"Note `{name}` could not be found!");
                return;
            }

            await Context.Channel.SendEmbedAsync($"{noteToShow.Name}", $"{noteToShow.Text}", await _serverService.GetServer(Context.Guild), ImageLookupUtility.GetImageUrl("NOTE_IMAGES"));
            //await ReplyAsync(noteToShow.Text);
        }
    }
}
