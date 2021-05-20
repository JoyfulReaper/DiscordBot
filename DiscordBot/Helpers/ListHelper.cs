using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Helpers
{
    public static class ListHelper
    {
        private static readonly Random _random = new();

        public static T RandomItem<T>(this List<T> list)
        {
            return list[_random.Next(list.Count)];
        }
    }
}
