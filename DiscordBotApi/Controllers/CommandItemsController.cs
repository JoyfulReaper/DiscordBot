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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DiscordBotApiLib.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotApiLib.Models;
using DiscordBotApiLib.Dtos;

namespace DiscordBotApi.Controllers
{
    [Authorize]
    [Route("api/CommandItems")]
    [ApiController]
    public class CommandItemsController : ControllerBase
    {
        private readonly ICommandItemRepo _commandItemRepo;
        private readonly IMapper _mapper;

        public CommandItemsController(ICommandItemRepo commandItemRepo,
            IMapper mapper)
        {
            _commandItemRepo = commandItemRepo;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetCommandItemById")]
        public async Task<ActionResult<CommandItem>> GetCommandItemById(int id)
        {
            var commandItem = await _commandItemRepo.GetCommandItemById(id);
            if (commandItem == null)
            {
                return NotFound();
            }

            return Ok(commandItem);
        }

        [HttpGet("GuildId/{guildId}", Name = "GetCommandItemByGuildId")]
        public async Task<ActionResult<CommandItem>> GetCommandItemByGuildId([FromQuery] int page, ulong guildId)
        {
            IEnumerable<CommandItem> commandItem;

            if (page == 0)
            {
                commandItem = await _commandItemRepo.GetCommandItemsByGuildId(guildId);
            }
            else
            {
                commandItem = await _commandItemRepo.GetCommandItemsByGuildId(guildId, page);
            }
            if (commandItem == null || !commandItem.Any())
            {
                return NotFound();
            }

            return Ok(commandItem);
        }

        [HttpPost]
        public async Task<ActionResult<ServerLogItem>> CreateServerLogItem(CommandItemCreateDto commandItemCreate)
        {
            var commandItemModel = _mapper.Map<CommandItem>(commandItemCreate);

            await _commandItemRepo.CreateCommandItem(commandItemModel);
            await _commandItemRepo.SaveChanges();

            return CreatedAtRoute(nameof(GetCommandItemById), new { Id = commandItemModel.Id }, commandItemModel);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteServerLogItem(int id)
        {
            var commandItemFromDb = await _commandItemRepo.GetCommandItemById(id);
            if (commandItemFromDb == null)
            {
                return NotFound();
            }

            _commandItemRepo.DeleteCommandItem(commandItemFromDb);
            await _commandItemRepo.SaveChanges();

            return NoContent();
        }
    }
}
