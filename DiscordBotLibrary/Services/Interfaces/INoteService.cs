using DiscordBotLibrary.Models;

namespace DiscordBotLibrary.Services.Interfaces;

public interface INoteService
{
    Task DeleteNoteAsync(long noteId);
    Task<IEnumerable<Note>> GetNotesAsync(long userId);
    Task SaveNoteAsync(Note note);
}