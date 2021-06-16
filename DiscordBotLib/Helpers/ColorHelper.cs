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

using Discord;
using DiscordBotLib.Models;
using System;

namespace DiscordBotLib.Helpers
{
    public static class ColorHelper
    {
        private static readonly Random _random = new();

        public static bool isValidColor (string color)
        {
            var rgb = color.Split(",");
            if (rgb.Length != 3)
            {
                return false;
            }

            Color discordColor;
            try
            {
                discordColor = new Color(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public static Color RandomColor()
        {
            return new Color(_random.Next(256), _random.Next(256), _random.Next(256));
        }

        public static Color GetColor(Server server)
        {
            if (server == null || server.EmbedColor.RawValue == 0)
            {
                return RandomColor();
            }
            else
            {
                return server.EmbedColor;
            }
        }
    }
}
