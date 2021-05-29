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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// See https://docs.stillu.cc/guides/concepts/logging.html for some information about logging
// It's advised to wrap logging in a Task.Run so Discord.NET's gateway thread is not blocked
// while waiting for logging data to be written
// TODO: Write a logging helper than can be used with Task.Run and replace all logger calls with
// this, or look into other solutions.

namespace DiscordBot
{
    class Program
    {
        private static readonly Process lavaLink = new Process();
        private static readonly ILogger logger = Log.ForContext<Program>();
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            Bootstrap.SetupLogging();
            var host = Bootstrap.Initialize(args);

            using (host)
            {
                logger.Information("Starting DiscordBot");

                try
                {
                    StartLavaLink();

                    await host.StartAsync(cts.Token);
                    await host.WaitForShutdownAsync();

                    ExitCleanly();
                }
                catch (Exception ex)
                {
                    // Catch all exceptions if they aren't handeled anywhere else, log and exit.
                    logger.Error(ex, "Unhandeled Exception Caught!");

                    Console.WriteLine("Unhandeled Exception Caught!");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);

                    while (ex.InnerException != null)
                    {
                        ex = ex.InnerException;
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }
        }

        /// <summary>
        /// Attempt to kill the bot cleanly.
        /// </summary>
        /// <param name="exitCode">Exit code to pass to the OS</param>
        public static void ExitCleanly(int exitCode = 0)
        {
            logger.Warning("Stopping DiscordBot");
            Console.WriteLine("DiscordBot Quitting");

            logger.Information("Killing Lavalink Proccess");
            //lavaLink.Kill(true);

            cts.Cancel();
            //Environment.Exit(exitCode);
        }

        private static void StartLavaLink()
        {
            //logger.Information("Starting LavaLink");
            //lavaLink.StartInfo.UseShellExecute = false;
            //lavaLink.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
            //lavaLink.StartInfo.FileName = "java";
            //lavaLink.StartInfo.Arguments = @"-jar .\LavaLink\Lavalink.jar";
            //lavaLink.StartInfo.CreateNoWindow = true;
            //lavaLink.Start();
        }
    }
}
