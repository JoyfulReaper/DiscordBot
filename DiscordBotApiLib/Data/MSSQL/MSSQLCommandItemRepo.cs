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

using DiscordBotApiLib.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotApiLib.Data.MSSQL
{
    public class MSSQLCommandItemRepo : ICommandItemRepo
    {
        private readonly DiscordBotContext _context;

        public MSSQLCommandItemRepo(DiscordBotContext context)
        {
            _context = context;
        }

        public async Task CreateCommandItem(CommandItem item)
        {
            // TODO check if there is a better way of doing this
            if(item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var channel = await _context.Channel.SingleOrDefaultAsync(x => x.ChannelId == item.Channel.ChannelId);
            var guild = await _context.Guild.SingleOrDefaultAsync(x => x.GuildId == item.Guild.GuildId);
            var user = await _context.User.SingleOrDefaultAsync(x => x.UserId == item.User.UserId);

            if (channel != null)
            {
                item.Channel = channel;
            }

            if (guild != null)
            {
                item.Guild = guild;
            }

            if (user != null)
            {
                item.User = user;
            }

            _context.Add(item);
        }

        public void DeleteCommandItem(CommandItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _context.CommandItem.Remove(item);
        }

        public async Task<IEnumerable<CommandItem>> GetAllCommandItems()
        {
            return await _context.CommandItem.ToListAsync();
        }

        public async Task<CommandItem> GetCommandItemById(int id)
        {
            return await _context.CommandItem
                .Include(c => c.Channel)
                .Include(c => c.Guild)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<CommandItem>> GetCommandItemsByGuildId(ulong guildId)
        {
            return await _context.CommandItem
                .Include(c => c.Channel)
                .Include(c => c.Guild)
                .Include(c => c.User)
                .Where(c => c.Guild.GuildId == guildId).ToListAsync();
        }

        public async Task<IEnumerable<CommandItem>> GetCommandItemsByGuildId(ulong guildId, int page, int pageSize = 25)
        {
            return await _context.CommandItem
                .Include(c => c.Channel)
                .Include(c => c.Guild)
                .Include(c => c.User)
                .Where(c => c.Guild.GuildId == guildId)
                .OrderByDescending(id => id.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> SaveChanges()
        {
            return await _context.SaveChangesAsync() >= 0;
        }

        public void UpdateCommandItem(CommandItem item)
        {
            // Handeled automaticlly by EF Core :)
        }
    }
}
