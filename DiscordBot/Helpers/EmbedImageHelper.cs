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

using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Helpers
{
    public static class EmbedImageHelper
    {
        public static readonly string[] BAN_IMAGES = new string[] { "https://image.freepik.com/free-vector/iron-ban-hammer-isolated-white_175250-450.jpg",
            "https://i.ytimg.com/vi/nnb2gXr4nCw/maxresdefault.jpg", "https://www.clipartkey.com/mpngs/m/7-78285_banned-hammer-png-clipart-free-library-ban-hammer.png"};

        public static readonly string[] UNBAN_IMAGES = new string[] { "https://tengaged.com/img_p/3993530.jpg?c=0",
            "https://latestnews.fresherslive.com/images/articles/origin/2021/01/20/how-to-get-unbanned-from-tinder-60081e1c387aa-1611144732.jpg" };

        private static Random _random = new Random();


        public static string GetImageUrl(string key)
        {
            Type type = typeof(EmbedImageHelper);
            var fieldInfo = type.GetField(key);

            if(fieldInfo == null)
            {
                return null;
            }

            string[] output = (string[])fieldInfo.GetValue(null);
            return output[_random.Next(output.Length)];
        }
    }
}
