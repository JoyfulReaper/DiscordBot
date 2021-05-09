using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Helpers
{
    public static class ColorHelper
    {
        private static Random _random = new();

        public static Color RandomColor()
        {
            return new Color(_random.Next(256), _random.Next(256), _random.Next(256));
        }
    }
}
