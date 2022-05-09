﻿/*
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

using Discord.WebSocket;
using DiscordBotLibrary.Models;
using DiscordBotLibrary.Repositories.Interfaces;
using DiscordBotLibrary.Services.Interfaces;

namespace DiscordBotLibrary.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly DiscordSocketClient _client;

    public UserService(IUserRepository userRepository,
        DiscordSocketClient client)
    {
        _userRepository = userRepository;
        _client = client;
    }
    
    public async Task<User?> LoadUserAsync(ulong userId)
    {
        var user = await _userRepository.LoadUserAsync(userId);

        if (user == null)
        {
            user = new User
            {
                DiscordUserId = userId,
                UserName = (await _client.GetUserAsync(userId)).Username
            };
            
            await _userRepository.SaveUserAsync(user);
        }
        
        if(user.UserName != (await _client.GetUserAsync(userId)).Username)
        {
            await _userRepository.SaveUserAsync(user);
        }

        return user;
    }
    
    public Task SaveUserAsync(User user)
    {
        return _userRepository.SaveUserAsync(user);
    }

}
