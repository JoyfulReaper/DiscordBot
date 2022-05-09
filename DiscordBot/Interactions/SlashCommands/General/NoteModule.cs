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
    private readonly BotInformation _botinfo;

    public NoteModule(IUserService userService,
        INoteService noteService,
        IConfiguration config)
    {
        _userService = userService;
        _noteService = noteService;
        _config = config;

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
}
