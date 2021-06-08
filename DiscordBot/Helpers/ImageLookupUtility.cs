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

namespace DiscordBot.Helpers
{
    public static class ImageLookupUtility
    {
        public static readonly string[] BAN_IMAGES = new string[] { "https://image.freepik.com/free-vector/iron-ban-hammer-isolated-white_175250-450.jpg",
            "https://i.ytimg.com/vi/nnb2gXr4nCw/maxresdefault.jpg", "https://www.clipartkey.com/mpngs/m/7-78285_banned-hammer-png-clipart-free-library-ban-hammer.png"};

        public static readonly string[] UNBAN_IMAGES = new string[] { "https://tengaged.com/img_p/3993530.jpg?c=0",
            "https://latestnews.fresherslive.com/images/articles/origin/2021/01/20/how-to-get-unbanned-from-tinder-60081e1c387aa-1611144732.jpg" };

        public static readonly string[] QUIT_IMAGES = new string[] { "https://i.makeagif.com/media/11-18-2014/2oMnrI.gif", "https://news.efinancialcareers.com/binaries/content/gallery/efinancial-careers/articles/2017/11/I-quit_twinsterphoto_GettyImages.jpg",
            "https://myzol.co.zw/Data/Articles/2387/bye-quit__zoom.png", "https://c1.staticflickr.com/3/2199/3527660157_2827558f95_z.jpg", "https://cdn.tinybuddha.com/wp-content/uploads/2015/08/Bad-Day.png",
            "https://geekologie.com/2015/08/03/dead-hitchhiking-robot.jpg"};

        public static readonly string[] BADCOMMAND_IMAGES = new string[] { "https://www.wheninmanila.com/wp-content/uploads/2017/12/meme-kid-confused.png" };

        public static readonly string[] LOGGING_IMAGES = new string[] { "https://cdn.quotesgram.com/img/87/86/1090166097-captains_log_meme.jpg" };

        private static Random _random = new Random();

        public static string GetImageUrl(string key)
        {
            Type type = typeof(ImageLookupUtility);
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
