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
using System.Threading.Tasks;

namespace DiscordBotApiLib.Data.Repositories
{
    public class ServerLogItemRepo : IServerLogItemRepo
    {
        private readonly DiscordBotContext _context;

        public ServerLogItemRepo(DiscordBotContext context)
        {
            _context = context;
        }

        public async Task CreateServerLogItem(ServerLogItem item)
        {
            // TODO check if there is a better way of doing this

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            var channel = await _context.Channel.SingleOrDefaultAsync(x => x.ChannelId == item.Channel.ChannelId);
            var guild = await _context.Guild.SingleOrDefaultAsync(x => x.GuildId == item.Guild.GuildId);

            if(channel != null)
            {
                item.Channel = channel;
            }

            if(guild != null)
            {
                item.Guild = guild;
            }

            _context.Add(item);
        }

        public void DeleteServerLogItem(ServerLogItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _context.ServerLogItem.Remove(item);
        }

        public async Task<IEnumerable<ServerLogItem>> GetAllServerLogItems()
        {
            return await _context.ServerLogItem.ToListAsync();
        }

        public async Task<ServerLogItem> GetServerLogItemById(int id)
        {
            return await _context.ServerLogItem
                .Include(l => l.Guild)
                .Include(l => l.Channel)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<ServerLogItem>> GetServerLogItemsByGuildId(ulong guildId)
        {
            return await _context.ServerLogItem
                .Include(l => l.Guild)
                .Include(l => l.Channel)
                .Where(l => l.Guild.GuildId == guildId).ToListAsync();
        }

        public async Task<IEnumerable<ServerLogItem>> GetServerLogItemsByGuildId(ulong guildId, int page, int pageSize = 25)
        {
            return await _context.ServerLogItem
                .Include(l => l.Guild)
                .Include(l => l.Channel)
                .Where(l => l.Guild.GuildId == guildId)
                .OrderByDescending(id => id.Id)
                .Skip((page -1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> SaveChanges()
        {
            return await _context.SaveChangesAsync() >= 0;
        }

        public void UpdateServerLogItem(ServerLogItem item)
        {
            // Handeled automaticlly by EF Core :)
        }
    }
}
