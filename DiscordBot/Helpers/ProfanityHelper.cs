using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Models;
using ProfanityFilter;


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
            "vulva", "wazoo", "weed", "weiner", "wedgie", "whiz", "womb", "rum"};

            foreach (var word in allowed)
            {
                filter.RemoveProfanity(word);
            }
        }

        public static bool ContainsProfanity(string sentence)
        {
            return filter.ContainsProfanity(sentence);
        }

        public static void HandleProfanity(SocketMessage message, Server server)
        {
            message.Channel.SendMessageAsync($"Lol, {message.Author.Username} said a swear!");
        }

        public static ReadOnlyCollection<string> GetProfanity(string sentence)
        {
            return filter.DetectAllProfanities(sentence);
        }
    }
}
