using DiscordBotLibrary.Models;
using DiscordBotLibrary.Repositories.Interfaces;
using DiscordBotLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Services;
public class NoteService : INoteService
{
    private readonly INoteRepository _noterepository;

    public NoteService(INoteRepository noterepository)
    {
        _noterepository = noterepository;
    }

    public Task SaveNoteAsync(Note note)
    {
        return _noterepository.SaveNoteAsync(note);
    }

    public Task<IEnumerable<Note>> GetNotesAsync(long userId)
    {
        return _noterepository.GetNotesAsync(userId);
    }

    public Task DeleteNoteAsync(long noteId)
    {
        return _noterepository.DeleteNoteAsync(noteId);
    }
}