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

using DiscordBotApiWrapper.Dtos;
using DiscordBotApiWrapper.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscordBotApiWrapper
{
    public class CommandItemApi : ICommandItemApi
    {
        private readonly ApiClient _client;
        private readonly ILogger<CommandItemApi> _logger;

        public CommandItemApi(ApiClient client,
            ILogger<CommandItemApi> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<HttpStatusCode> DeleteCommandItem(int id)
        {
            var result = await _client.DeleteAsync($"/api/CommandItems/{id}");
            _logger.LogDebug("Delete {id}: Result {result}", id, (int)result);
            return result;
        }

        public async Task<IEnumerable<CommandItem>> GetCommandItemsForGuild(int guildId)
        {
            var result = await _client.GetAsync<IEnumerable<CommandItem>>($"/api/CommandItems/GuildId/{guildId}");
            _logger.LogDebug("Get {guildId}: Count {result}", guildId, result.Count());
            return result;
        }

        public async Task<IEnumerable<CommandItem>> GetCommandItemsForGuild(int guildId, int page)
        {
            var result = await _client.GetAsync<IEnumerable<CommandItem>>($"/api/CommandItems/GuildId/{guildId}?page={page}");
            _logger.LogDebug("Get {guildId}, Page {page}: Count {result}", guildId, page, result.Count());
            return result;
        }

        public async Task<CommandItem> GetCommandItem(int id)
        {
            var result =  await _client.GetAsync<CommandItem>($"/api/CommandItem/{id}");
            _logger.LogDebug("Get {id}", id);
            return result;
        }

        public async Task<HttpStatusCode> SaveCommandItem(CommandItemCreateDto item)
        {
            string jsonString = JsonSerializer.Serialize(item);
            var statusCode = await _client.PostAsync("/api/CommandItems/", item);

            if (statusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Your username or password is incorrect!");
            }
            _logger.LogDebug("Posting JSON\n {json}:\nresult: {result}", jsonString, (int)statusCode);
            return statusCode;
        }
    }
}
