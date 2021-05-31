﻿/*
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
using System.Diagnostics;
using System.IO;
using Serilog;

namespace DiscordBot.Helpers
{
    class LavaLinkHelper
    {
        private static Process _lavaLink = new Process();

        public static void StartLavaLink()
        {
            Log.Information("Starting LavaLink");
            _lavaLink.StartInfo.UseShellExecute = false;
            _lavaLink.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            _lavaLink.StartInfo.FileName = "java";
            _lavaLink.StartInfo.Arguments = @"-jar Lavalink.jar";
            _lavaLink.StartInfo.CreateNoWindow = true;
            _lavaLink.Start();
        }

        public static void StopLavaLink()
        {
            Log.Information("Killing LavaLink");
            _lavaLink.Kill(true);
        }

        public static bool isLavaLinkRunning()
        {
            try
            {
                var id = _lavaLink.Id;
            }
            catch(InvalidOperationException)
            {
                return false;
            }

            return !_lavaLink.HasExited;
        }
    }
}
