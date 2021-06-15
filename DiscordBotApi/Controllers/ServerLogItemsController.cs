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

using AutoMapper;
using DiscordBotApi.Data;
using DiscordBotApi.Dtos;
using DiscordBotApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotApi.Controllers
{
    [Authorize]
    [Route("api/ServerLogItems")]
    [ApiController]
    public class ServerLogItemsController : ControllerBase
    {
        private readonly IServerLogItemRepo _serverLogItemRepo;
        private readonly IMapper _mapper;

        public ServerLogItemsController(IServerLogItemRepo serverLogItemRepo,
            IMapper mapper)
        {
            _serverLogItemRepo = serverLogItemRepo;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name="GetServerLogItemById")]
        public async Task<ActionResult<ServerLogItem>> GetServerLogItemById(int id)
        {
            var serverLogItem = await _serverLogItemRepo.GetServerLogItemById(id);
            if(serverLogItem == null)
            {
                return NotFound();
            }

            return Ok(serverLogItem);
        }

        [HttpGet("GuildId/{guildId}", Name = "GetServerLogItemByGuildId")]
        public async Task<ActionResult<ServerLogItem>> GetServerLogItemByGuildId([FromQuery] int page, ulong guildId)
        {
            IEnumerable<ServerLogItem> serverLogItems;

            if (page == 0)
            {
                serverLogItems = await _serverLogItemRepo.GetServerLogItemsByGuildId(guildId);
            }
            else
            {
                serverLogItems = await _serverLogItemRepo.GetServerLogItemsByGuildId(guildId, page);
            }
            if (serverLogItems == null || !serverLogItems.Any())
            {
                return NotFound();
            }

            return Ok(serverLogItems);
        }

        [HttpPost]
        public async Task<ActionResult<ServerLogItem>> CreateServerLogItem(ServerLogItemCreateDto serverLogItemCreateDto)
        {
            var serverLogItemModel = _mapper.Map<ServerLogItem>(serverLogItemCreateDto);

            _serverLogItemRepo.CreateServerLogItem(serverLogItemModel);
            await _serverLogItemRepo.SaveChanges();

            return CreatedAtRoute(nameof(GetServerLogItemById), new { Id = serverLogItemModel.Id }, serverLogItemModel);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteServerLogItem(int id)
        {
            var serverLogItemFromDb = await _serverLogItemRepo.GetServerLogItemById(id);
            if(serverLogItemFromDb == null)
            {
                return NotFound();
            }

             _serverLogItemRepo.DeleteServerLogItem(serverLogItemFromDb);
            await _serverLogItemRepo.SaveChanges();

            return NoContent();
        }
    }
}
