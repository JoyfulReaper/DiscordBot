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


using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Initial Logging for before the Dependency Injection is setup
            Bootstrap.SetupLogging();

            // Set up Dependency Injection
            var serviceProvider = Bootstrap.Initialize(args);
            var chatService = serviceProvider.GetRequiredService<IChatService>();

            // Static logger
            ILogger logger = Log.ForContext<Program>();
            
            var cts = new CancellationTokenSource();

            if (chatService != null)
            {
                try
                {
                    // Start the DiscordBot
                    logger.Information("Starting chatService");
                    await Task.Run(chatService.Start, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    logger.Warning("Cancelation Requested");
                    Console.WriteLine("Cancelation was requested");
                }
                catch(Exception e)
                {
                    // Catch all exceptions if they aren't handeled anywhere else, log and exit.

                    logger.Error(e, "Unhandeled Exception Caught!");

                    Console.WriteLine("Unhandeled Exception Caught!");
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);

                    while(e.InnerException != null)
                    {
                        e = e.InnerException;
                        Console.WriteLine(e.Message);
                        Console.WriteLine(e.StackTrace);
                    }

                    Environment.Exit(1);
                }
            }
            else
            {
                logger.Fatal("Failed to retrieve ChatService!");
                Console.WriteLine("Failed to retrieve ChatService!");
                Environment.Exit(1);
            }

            while(true)
            {
                QuitOnQPress(cts);
            }
        }

        private static void QuitOnQPress(CancellationTokenSource cts)
        {
            // If the "Q" key is pressed quit the bot!
            var key = Console.ReadKey(true).KeyChar;

            if(char.ToLowerInvariant(key) == 'q')
            {
                Console.WriteLine("Quiting!");
                cts.Cancel();
                Environment.Exit(0);
            }
        }
    }
}
