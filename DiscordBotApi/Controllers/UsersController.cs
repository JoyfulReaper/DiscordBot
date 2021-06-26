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
using DiscordBotApiLib.Data;
using DiscordBotApiLib.Dtos;
using DiscordBotApiLib.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotApi.Controllers
{
    [Authorize]
    [Route("api/User")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepo _userRepository;
        private readonly IMapper _mapper;

        public UsersController(IUserRepo userRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [HttpGet("{id}", Name = "GetUserById")]
        public async Task<ActionResult<User>> GetUserById(int id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("UserId/{id}", Name = "GetUserByDiscordId")]
        public async Task<ActionResult<User>> GetUserByDiscordId(ulong id)
        {
            var user = await _userRepository.GetUserByDiscordUserId(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("UserName/{username}", Name = "GetUserByDiscordUsername")]
        public async Task<ActionResult<User>> GetUserByDiscordUsername(string username)
        {
            var user = await _userRepository.GetUserByDiscordUserName(username);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        //TODO: Database changes required
        //[HttpGet("GuildId/{guildId}", Name = "GetUsersByGuildId")]
        //public async Task<ActionResult<CommandItem>> GetUsersByGuildId([FromQuery] int page, ulong guildId)
        //{
        //    IEnumerable<CommandItem> commandItem;

        //    if (page == 0)
        //    {
        //        commandItem = await _userRepository.GetUsersByGuildId(guildId);
        //    }
        //    else
        //    {
        //        commandItem = await _userRepository.GetUsersByGuild(guildId, page);
        //    }
        //    if (commandItem == null || !commandItem.Any())
        //    {
        //        return NotFound();
        //    }

        //    return Ok(commandItem);
        //}

        [HttpPost]
        public async Task<ActionResult<ServerLogItem>> CreateUser(UserCreateDto userCreateDto)
        {
            var UserModel = _mapper.Map<User>(userCreateDto);

            _userRepository.CreateUser(UserModel);
            await _userRepository.SaveChanges();

            return CreatedAtRoute(nameof(GetUserById), new { Id = UserModel.Id }, UserModel);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteServerLogItem(int id)
        {
            var UserFromDb = await _userRepository.GetUserById(id);
            if (UserFromDb == null)
            {
                return NotFound();
            }

            _userRepository.DeleteUser(UserFromDb);
            await _userRepository.SaveChanges();

            return NoContent();
        }
    }
}
