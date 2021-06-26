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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBotApiLib.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotApiLib.Data.Repositories
{
    public class UserRepo : IUserRepo
    {
        private readonly DiscordBotContext _context;

        public UserRepo(DiscordBotContext context)
        {
            _context = context;
        }

        public void CreateUser(User item)
        {
            // TODO check if there is a better way of doing this
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _context.Add(item);
        }

        public void DeleteUser(User item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _context.User.Remove(item);
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _context.User.ToListAsync();
        }

        public async Task<User> GetUserByDiscordUserId(ulong userId)
        {
            return await _context.User
               .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User> GetUserByDiscordUserName(string userName)
        {
            return await _context.User
               .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<User> GetUserById(int id)
        {
            return await _context.User
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        // TODO: This will require some DB Changes
        //public async Task<IEnumerable<User>> GetUserGuildId(ulong guildId)
        //{
        //    return await _context.User
        //        .Where(u => u.Guild.GuildId == guildId).ToListAsync();
        //}

        // TODO: This will require some DB changes
        //public async Task<IEnumerable<User>> GetUsersByGuildId(ulong guildId, int page, int pageSize = 25)
        //{
        //    return await _context.User
        //        .Where(c => u.Guild.GuildId == guildId)
        //        .OrderByDescending(id => id.Id)
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();
        //}

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
