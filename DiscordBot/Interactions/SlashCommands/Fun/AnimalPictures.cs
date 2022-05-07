/*
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

using Discord;
using Discord.Interactions;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;
using System.Net.Http.Json;

namespace DiscordBot.Interactions.SlashCommands.Fun;
public class AnimalPictures : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IGuildService _guildService;

    public AnimalPictures(IHttpClientFactory httpClientFactory,
        IGuildService guildService)
    {
        _httpClientFactory = httpClientFactory;
        _guildService = guildService;
    }

    [SlashCommand("dog", "Dog pictures")]
    public async Task<ExecuteResult> Dog()
    {
        var client = _httpClientFactory.CreateClient();
        var dog = await client.GetFromJsonAsync<DogResponse>("https://dog.ceo/api/breeds/image/random");

        if (dog == null ||  dog.Status != "success")
        {
            return ExecuteResult.FromError(InteractionCommandError.Unsuccessful, $"{nameof(Dog)}: The API did not return a successful response");
        }

        await RespondAsync(embed: EmbedHelper.GetEmbed("A cute fluffly dog!", color: await _guildService.GetEmbedColorAsync(Context), imageUrl: dog.Message));
    }
    
    private class DogResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }}
