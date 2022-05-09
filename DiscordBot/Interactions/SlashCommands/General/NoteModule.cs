/*
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

using Discord.Interactions;
using DiscordBotLibrary.Attributes;
using DiscordBotLibrary.ConfigSections;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Models;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Interactions.SlashCommands.General;

[Group("note", "User notes")]
public class NoteModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IUserService _userService;
    private readonly INoteService _noteService;
    private readonly IConfiguration _config;
    private readonly IGuildService _guildService;
    private readonly BotInformation _botinfo;

    public NoteModule(IUserService userService,
        INoteService noteService,
        IConfiguration config,
        IGuildService guildService)
    {
        _userService = userService;
        _noteService = noteService;
        _config = config;
        _guildService = guildService;
        _botinfo = _config.GetSection("BotInformation").Get<BotInformation>();
    }

    [SlashCommand("create", "Create a new note")]
    public async Task NoteCreate([MaxLength(30)] string name, [MaxLength(300)] string text)
    {
        var user = await _userService.LoadUserAsync(Context.User.Id);
        var notes = await _noteService.GetNotesAsync(user.UserId);

        if (notes.Count() >= _botinfo.MaxUserNotes)
        {
            await RespondAsync($"You have reached the maximum number of notes (`{_botinfo.MaxUserNotes}`). Please delete one first!");
        }

        if (notes.Any(n => n.Name.ToLowerInvariant() == name.ToLowerInvariant()))
        {
            await RespondAsync($"You already have a note named `{name}` please pick a different name.");
            return;
        }

        Note note = new Note()
        {
            Name = name,
            Text = text,
            UserId = user.UserId
        };

        await _noteService.SaveNoteAsync(note);
        await RespondAsync($"Note `{name}` created!"); 
    }

    [SlashCommand("list", "list user notes")]
    public async Task NoteList()
    {
        await Context.Channel.TriggerTypingAsync();

        var user = await _userService.LoadUserAsync(Context.User.Id);
        var notes = (await _noteService.GetNotesAsync(user.UserId)).ToList();

        if (notes == null || !notes.Any())
        {
            await RespondAsync("You haven't created any notes yet!");
            return;
        }

        string output = string.Empty;
        for (int i = 0; i < notes.Count(); i++)
        {
            output += i + 1 + ") " + notes[i].Name + "\n";
        }

        await RespondAsync(output);
    }


    [SlashCommand("delete", "delete a note")]
    public async Task NoteDelete([Summary("name")] string name)
    {
        await Context.Channel.TriggerTypingAsync();

        var user = await _userService.LoadUserAsync(Context.User.Id);
        var notes = (await _noteService.GetNotesAsync(user.UserId));
        var noteToDelete = notes?.Where(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant()).SingleOrDefault();

        if (noteToDelete == null)
        {
            await RespondAsync($"Note `{name}` could not be found!");
            return;
        }

        await _noteService.DeleteNoteAsync(noteToDelete.NoteId);
        await RespondAsync($"Deleted `{name}`");
    }

    [SlashCommand("read", "read note")]
    public async Task NoteRead(string name)
    {
        await Context.Channel.TriggerTypingAsync();

        var user = await _userService.LoadUserAsync(Context.User.Id);
        var notes = await _noteService.GetNotesAsync(user.UserId);
        var noteToShow = notes?.Where(x => x.Name.ToLowerInvariant() == name.ToLowerInvariant()).SingleOrDefault();

        if (noteToShow == null)
        {
            await RespondAsync($"Note `{name}` could not be found!");
            return;
        }

        await RespondAsync(embed: EmbedHelper.GetEmbed($"{noteToShow.Name}", $"{noteToShow.Text}",
            await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.NOTE_IMAGES))));
    }
}
