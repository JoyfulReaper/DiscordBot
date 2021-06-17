/*
MIT License

Copyright (c) 2020 Hendrik Demir(Directoire)

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

// Code is from Discord.NET Bot Development · Generating images · Episode 7
// https://www.youtube.com/watch?v=_-_QyGizXrA&list=PLaqoc7lYL3ZDCDT9TcP_5hEKuWQl7zudR&index=7

using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBotLib.Services
{
    public class BannerImageService
    {
        private readonly ILogger<BannerImageService> _logger;

        public BannerImageService(ILogger<BannerImageService> logger)
        {
            _logger = logger;
        }

        public async Task<MemoryStream> CreateImage(SocketGuildUser user, string url = null)
        {
            if(url == null)
            {
                url = "https://images.unsplash.com/photo-1500829243541-74b677fecc30?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=2555&q=80";
            }

            var avatar = await FetchImage(user.GetAvatarUrl(size: 2048, format: Discord.ImageFormat.Png) ?? user.GetDefaultAvatarUrl());
            var background = await FetchImage(url);

            background = CropToBanner(background);
            avatar = ClipImageToCircle(avatar);

            var bitmap = avatar as Bitmap;
            bitmap?.MakeTransparent();

            var banner = CopyRegionIntoImage(bitmap, background);
            banner = DrawTextToImage(banner, $"{user.Username}#{user.Discriminator} joined the server", $"Member #{ user.Guild.MemberCount}");

            var memoryStream = new MemoryStream();
            banner.Save(memoryStream, ImageFormat.Png);
            return await Task.FromResult(memoryStream);
        }

        private Image DrawTextToImage(Image image, string header, string subheader)
        {
            // TODO deal with the font not existing

            var roboto = new Font("Roboto", 30, FontStyle.Regular);
            var robotoSmall = new Font("Roboto", 23, FontStyle.Regular);

            var brushWhite = new SolidBrush(Color.White);
            var brushGrey = new SolidBrush(ColorTranslator.FromHtml("#B3B3B3"));

            var headerX = image.Width / 2;
            var headerY = (image.Height / 2) + 115;

            var subHeaderX = image.Width / 2;
            var subHeaderY = (image.Height / 2) + 160;

            var drawFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            using var grD = Graphics.FromImage(image);
            grD.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            grD.DrawString(header, roboto, brushWhite, headerX, headerY, drawFormat);
            grD.DrawString(subheader, robotoSmall, brushGrey, subHeaderX, subHeaderY, drawFormat);

            var img = new Bitmap(image);
            return img;
        }

        private Image CopyRegionIntoImage(Image source, Image destination)
        {
            using var grD = Graphics.FromImage(destination);
            var x = (destination.Width / 2) - 110;
            var y = (destination.Height / 2) - 155;

            grD.DrawImage(source, x, y, 220, 220);
            return destination;
        }

        private Image ClipImageToCircle(Image image)
        {
            Image destination = new Bitmap(image.Width, image.Height, image.PixelFormat);
            var radius = image.Width / 2;
            var x = image.Width / 2;
            var y = image.Height / 2;

            using Graphics g = Graphics.FromImage(destination);
            var r = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Brush brush = new SolidBrush(Color.Transparent))
            {
                g.FillRectangle(brush, 0, 0, destination.Width, destination.Height);
            }

            var path = new GraphicsPath();
            path.AddEllipse(r);
            g.SetClip(path);
            g.DrawImage(image, 0, 0);

            return destination;
        }

        private async Task<Image> FetchImage(string url)
        {
            var client = new HttpClient();
            var response = await client.GetAsync(url);

            if(!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Unable to fetch: {url}", url);

                var backupResponse = await client.GetAsync("https://images.unsplash.com/photo-1500829243541-74b677fecc30?ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&ixlib=rb-1.2.1&auto=format&fit=crop&w=2555&q=80");
                var backupStream = await backupResponse.Content.ReadAsStreamAsync();
                return Image.FromStream(backupStream);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            return Image.FromStream(stream);
        }

        private static Bitmap CropToBanner(Image image)
        {
            var originalWidth = image.Width;
            var originalHeight = image.Height;
            var destinationSize = new Size(1100, 450);

            var heightRatio = (float)originalHeight / destinationSize.Height;
            var widthRation = (float)originalWidth / destinationSize.Width;

            var ratio = Math.Min(heightRatio, widthRation);

            var heightScale = Convert.ToInt32(destinationSize.Height * ratio);
            var widthScale = Convert.ToInt32(destinationSize.Width * ratio);

            var startX = (originalWidth - widthScale) / 2;
            var startY = (originalHeight - heightScale) / 2;

            var sourceRectangle = new Rectangle(startX, startY, widthScale, heightScale);
            var bitmap = new Bitmap(destinationSize.Width, destinationSize.Height);
            var destinationRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            using var g = Graphics.FromImage(bitmap);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);

            return bitmap;
        }
    }
}
