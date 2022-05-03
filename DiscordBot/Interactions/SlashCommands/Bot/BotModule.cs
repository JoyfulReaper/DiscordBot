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

using Discord.Interactions;
using DiscordBotLibrary.ConfigSections;
using DiscordBotLibrary.Helpers;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Interactions.SlashCommands.Bot
{
    public class BotModule : InteractionModuleBase
    {
        private readonly IConfiguration _config;
        private readonly BotInformation _botInfo;

        public BotModule(IConfiguration configuration)
        {
            _config = configuration;
            _botInfo = _config.GetSection("BotInformation").Get<BotInformation>();
        }

        [SlashCommand("invite", "Invite the bot to your server")]
        public async Task Invite()
        {
            await RespondAsync("test",  EmbedHelper.GetEmbedAsArray("Invite", $"Please click on the link to invite me to your server!\n{_botInfo.InviteLink}",
                thumbImage: ImageLookup.GetImageUrl("INVITE_IMAGES")));
        }
    }
}
