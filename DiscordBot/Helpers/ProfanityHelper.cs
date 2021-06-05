using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Models;


namespace DiscordBot.Helpers
{
    public class ProfanityHelper
    {
        // WTF would they make the Namespace and class name the same?
        public static ProfanityFilter.ProfanityFilter filter = new ProfanityFilter.ProfanityFilter();

        static ProfanityHelper()
        {
            // These words aren't really profanity, remove them from the filter
            // Probably missed a bunch

            var allowed = new[] { "butt", "poop", "bender", "bugger", "bum", "drunk", "dummy", "foobar", "gays", "hardcore", "hookah", "hun", "lesbian", "lesbians",
            "piss", "pissed", "porn", "pornography", "potty", "prod", "psycho", "pube", "pubes", "pubic", "queer", "reefer", "sex", "sexual", "stoned", "suck", "sucks",
            "sucking", "sucked", "tampon", "tart", "testical", "testicle", "thrust", "thug", "tit", "toke", "toots", "topless", "trashy", "turd", "ugly", "urine",
            "balls", "barf", "big breasts", "big black", "big tits", "bloody", "boob", "boobs", "bong", "booger", "boong", "booze", "bummer", "cervix", "climax", 
            "condom", "crack", "crap", "crappy", "dirty", "erect", "erotic", "fart", "fubar", "ganja", "genitals", "god", "hemp", "jerk", "labia", "lmao", "lmfao",
            "maxi", "meth", "moron", "nipple", "nipples", "omg", "opiate", "opium", "organ", "orally", "orgasm", "pcp", "panty", "pee", "penetrate", "penetration",
            "penis", "queers", "rectal", "rectum", "taste my", "teste", "testes", "tied up", "undies", "unwed", "urinal", "vagina", "virgin", "vomit", "vodka",
            "vulva", "wazoo", "weed", "weiner", "wedgie", "whiz", "womb", "rum", "kill", "murder", "stupid", "flaps"};

            foreach (var word in allowed)
            {
                filter.RemoveProfanity(word);
            }
        }

        public static bool ContainsProfanity(string sentence)
        {
            return filter.ContainsProfanity(sentence);
        }

        public async static Task HandleProfanity(SocketUserMessage message, Server server, IReadOnlyList<string> badWords)
        {
            //await message.Channel.SendMessageAsync($"Lol, {message.Author.Username} said a swear!");

            var channel = message.Channel as SocketGuildChannel;
            var guild = channel.Guild;
            var loggingChannel = guild.GetChannel(server.LoggingChannel);
            var badWordsJoined = String.Join(", ", badWords);

            if (loggingChannel != null)
            {
                await (loggingChannel as SocketTextChannel).SendLogAsync("Profanity Filter", $"{message.Author.Mention} said a bad word: {message.Content}\nin {channel.Guild.Name}/{channel.Name}.\nWords: `{badWordsJoined}`",
                    ColorHelper.GetColor(server));
            }

            var censored = filter.CensorString(message.Content);
            await message.DeleteAsync();

            await (channel as SocketTextChannel).SendMessageAsync($"{message.Author.Mention}, please don't swear. Orignial message: {censored.Replace("*", "#")}");
        }

        public static ReadOnlyCollection<string> GetProfanity(string sentence)
        {
            return filter.DetectAllProfanities(sentence);
        }
    }
}
