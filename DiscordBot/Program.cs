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

Some elements of this bot were insipred by the Discord.NET Bot Development Series:
https://www.youtube.com/playlist?list=PLaqoc7lYL3ZDCDT9TcP_5hEKuWQl7zudR

Although most of the code is modified from the orginal 
(For example here we use Dapper vs EF and SQLite vs MySQL)
any code used from the series is licensed under MIT as well:
https://github.com/Directoire/dnbds
*/


using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        private static readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private static readonly Process _lavaLink = new Process();
        private static readonly ILogger _logger = Log.ForContext<Program>();
        private static bool _startLavaLink = false;

        static async Task Main(string[] args)
        {
            // Initial Logging for before the Dependency Injection is setup
            Bootstrap.SetupLogging();

            // Set up Dependency Injection
            var serviceProvider = Bootstrap.Initialize(args);
            var chatService = serviceProvider.GetRequiredService<IChatService>();
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            if (!bool.TryParse(config.GetSection("StartLavaLink").Value, out _startLavaLink))
            {
                _logger.Warning("Unable to parse StartLavaLink, using {value}", _startLavaLink);
            }

            if (chatService != null)
            {
                try
                {
                    if (_startLavaLink)
                    {
                        StartLavaLink();
                    }

                    // Start the DiscordBot
                    _logger.Information("DiscordBot Starting");
                    await Task.Run(chatService.Start, _cts.Token);
                }
                catch(Exception e)
                {
                    // Catch all exceptions if they aren't handeled anywhere else, log and exit.
                    _logger.Error(e, "Unhandeled Exception Caught!");

                    Console.WriteLine("Unhandeled Exception Caught!");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);

                    while(e.InnerException != null)
                    {
                        e = e.InnerException;
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }

                    ExitCleanly();
                }
            }
            else
            {
                _logger.Fatal("Failed to retrieve ChatService!");
                Console.WriteLine("Failed to start DiscordBot!");

                ExitCleanly();
            }

            while(true)
            {
                // If the "Q" key is pressed quit the bot!
                Console.WriteLine("Press 'Q' to quit!");
                var key = Console.ReadKey(true).KeyChar;

                if (char.ToLowerInvariant(key) == 'q')
                {
                    break;
                }
            }
            ExitCleanly();
        }

        /// <summary>
        /// Attempt to kill the bot cleanly.
        /// </summary>
        /// <param name="exitCode">Exit code to pass to the OS</param>
        public static void ExitCleanly(int exitCode = 0)
        {
            Console.WriteLine("Discord Bot is quiting!");
            _cts.Cancel();

            if (_startLavaLink)
            {
                _logger.Information("Killing Lavalink Proccess");
                _lavaLink.Kill(true);
            }

            Environment.Exit(exitCode);
        }

        private static void StartLavaLink()
        {
            _logger.Information("Starting LavaLink");
            _lavaLink.StartInfo.UseShellExecute = false;
            _lavaLink.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            _lavaLink.StartInfo.FileName = "java";
            _lavaLink.StartInfo.Arguments = @"-jar Lavalink.jar";
            _lavaLink.StartInfo.CreateNoWindow = true;
            _lavaLink.Start();
        }
    }
}
