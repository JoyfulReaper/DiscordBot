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

using DiscordBotLib.Extensions;
using Serilog;
using System;

namespace DiscordBotLib.Helpers
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

        public static readonly string[] BADCOMMAND_IMAGES = new string[] { "https://www.wheninmanila.com/wp-content/uploads/2017/12/meme-kid-confused.png",
        "https://giphy.com/embed/xT0xeuOy2Fcl9vDGiA"};

        public static readonly string[] LOGGING_IMAGES = new string[] { "https://cdn.quotesgram.com/img/87/86/1090166097-captains_log_meme.jpg", "https://www.uncommongoods.com/images/items/49100/49180_3_640px.jpg" };

        public static readonly string[] EIGHTBALL_IMAGES = new string [] {
            "https://upload.wikimedia.org/wikipedia/commons/9/90/Magic8ball.jpg",
            "https://media1.tenor.com/images/821d79609a5bc1395d8dacab2ad8e8b6/tenor.gif?itemid=17798802",
            "https://media2.giphy.com/media/26xBJp4dcSdGxv2Zq/giphy.gif?cid=ecf05e47pnz54n4axc22ms430kzxmt8t1db6jm6qfm5vmg1p&rid=giphy.gif&ct=g" };

        public static readonly string[] COIN_IMAGES = new string[] { "https://www.bellevuerarecoins.com/wp-content/uploads/2013/11/bigstock-Coin-Flip-5807921.jpg" };

        public static readonly string[] DIE_IMAGES = new string[] { "https://miro.medium.com/max/1920/0*bLJxMZ_YS0RxF-82.jpg" };

        public static readonly string[] GUN_IMAGES = new string[] { "https://www.wealthmanagement.com/sites/wealthmanagement.com/files/styles/article_featured_standard/public/gun-one-bullet-russian-roulette.jpg?itok=Q55CNN7q" };

        public static readonly string[] MATH_IMAGES = new string[] { "https://i.pinimg.com/originals/97/a3/b9/97a3b92384b62eb04566a457f6d76f6c.gif" };

        public static readonly string[] KICK_IMAGES = new string[] { "https://www.nydailynews.com/resizer/vwH9gF1tqmXVFcROKQqcar7mL3U=/800x608/top/arc-anglerfish-arc2-prod-tronc.s3.amazonaws.com/public/CZ5U3VRUGH74ETHW7KVB7OZZTY.jpg" };

        public static readonly string[] PURGE_IMAGES = new string[] { "https://clipground.com/images/bye-clipart-17.jpg" };

        public static readonly string[] PREFIX_IMAGES = new string[] { "https://www.thecurriculumcorner.com/wp-content/uploads/2012/10/prefixposter.jpg" };

        public static readonly string[] ERROR_IMAGES = new string[] { "https://www.elegantthemes.com/blog/wp-content/uploads/2020/08/000-http-error-codes.png" };

        public static readonly string[] MUTE_IMAGES = new string[] { "https://image.freepik.com/free-vector/no-loud-sound-mute-icon_101884-1079.jpg" };

        public static readonly string[] UNMUTE_IMAGES = new string[] { "https://imgaz2.staticbg.com/thumb/large/oaupload/ser1/banggood/images/21/07/9474ae00-56ad-43ba-9bf1-97c7e80d34ee.jpg.webp" };

        public static readonly string[] SEARCH_IMAGES = new string[] { "https://www.computerhope.com/jargon/s/search-engine.jpg" };

        public static readonly string[] NOTE_IMAGES = new string[] { "https://cdn.osxdaily.com/wp-content/uploads/2016/03/notes-icon-mac.jpg" };

        public static readonly string[] HELP_IMAGES = new string[] { "http://2.bp.blogspot.com/-VOD5zQ-mHGE/VAxpj6tzZMI/AAAAAAAALx8/nVeA7AJe8Ek/s1600/help-emoticon.png", "https://farm3.staticflickr.com/2541/3971525059_b358acdd48_z.jpg" };

        public static readonly string[] BROADCAST_IMAGES = new string[] { "https://memegenerator.net/img/images/600x600/14916770/spongebob-yelling-at-squidward.jpg", "http://www.phillyemploymentlawyer.com/wp-content/uploads/2016/01/boss-yelling.jpg" };

        public static readonly string[] INVITE_IMAGES = new string[] { "https://i.etsystatic.com/5981074/r/il/1ceb12/820186932/il_1140xN.820186932_nv5d.jpg", "https://www.partyatlewis.com/Merchant2/graphics/00000001/33080.jpg" };

        private static readonly Random _random = new Random();

        public static string GetImageUrl(string key)
        {
            var logger = Log.ForContext(typeof(ImageLookupUtility));

            Type type = typeof(ImageLookupUtility);
            var fieldInfo = type.GetField(key);

            if(fieldInfo == null)
            {
                logger.Warning("Key: {key} not found!", key);
                return null;
            }

            string[] urlArray = (string[])fieldInfo.GetValue(null);
            string imageUrl = urlArray.RandomItem();

            logger.Information("Sending image: {imageurl}", imageUrl);

            return imageUrl;
        }
    }
}
