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
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Helpers
{
    public static class RPSHelper
    {
        private static Random _random = new();

        private enum ThrowResult
        {
            Rock,
            Paper,
            Scissors
        };

        private enum Winner 
        { 
            Bot, 
            Player, 
            Tie 
        };

        public async static Task RPSProcessor(Cacheable<IUserMessage, ulong> cachedEntity, ISocketMessageChannel channel, SocketReaction reaction)
        {
            var message = (SocketUserMessage) cachedEntity.Value;
            var reactingUser = (SocketUser)reaction.User;

            if(reactingUser.IsBot)
            {
                return;
            }

            if (message.Content == "Choose Rock, Paper, or Scissors!")
            {
                var reactions = message.Reactions;
                IEmote usersReaction = null;

                foreach (var r in reactions)
                {
                    if (r.Value.ReactionCount > 1)
                    {
                        usersReaction = r.Key;
                    }
                }

                ThrowResult playerThrow = ThrowResult.Rock;
                if (usersReaction.Name == "🪨")
                {
                    playerThrow = ThrowResult.Rock;
                }
                else if (usersReaction.Name == "🧻")
                {
                    playerThrow = ThrowResult.Paper;
                }
                else if (usersReaction.Name == "✂️")
                {
                    playerThrow = ThrowResult.Scissors;
                }
                else
                {
                    return;
                }

                ThrowResult computerThrow = EnumHelper.RandomEnumValue<ThrowResult>();
                var winner = DetermineWinner(computerThrow, playerThrow);

                string outputMessage;
                // TODO get the emoji from the Enum...
                if (winner == Winner.Bot)
                {
                    outputMessage = $"{ message.Author.Mention } *won* by throwing { Enum.GetName(typeof(ThrowResult), computerThrow)} against { usersReaction.Name}!";
                    
                }
                else if (winner == Winner.Player)
                {
                    outputMessage = $"{ reactingUser.Mention } *won* by throwing { usersReaction.Name} against { Enum.GetName(typeof(ThrowResult), computerThrow)}!";
                }
                else
                {
                    outputMessage = $"It's a tie! { Enum.GetName(typeof(ThrowResult), computerThrow)} vs { Enum.GetName(typeof(ThrowResult), playerThrow)}!";
                }

                await message.ModifyAsync(msg => msg.Content = outputMessage);
            }
        }

        private static Winner DetermineWinner(ThrowResult botResult, ThrowResult player)
        {
            Winner winner = (Winner)(-1);

            if (!EnumHelper.EnumValueIsValid(botResult))
            {
                throw new ArgumentException("Argument is invalid.", nameof(botResult));
            }
            if (!EnumHelper.EnumValueIsValid(player))
            {
                throw new ArgumentException("Argument is invalid.", nameof(player));
            }

            switch (botResult)
            {
                case ThrowResult.Rock:
                    switch (player)
                    {
                        case ThrowResult.Rock:
                            winner = Winner.Tie;
                            break;
                        case ThrowResult.Paper:
                            winner = Winner.Player;
                            break;
                        case ThrowResult.Scissors:
                            winner = Winner.Bot;
                            break;
                        default:
                            break;
                    }
                    break;
                case ThrowResult.Paper:
                    switch (player)
                    {
                        case ThrowResult.Rock:
                            winner = Winner.Bot;
                            break;
                        case ThrowResult.Paper:
                            winner = Winner.Tie;
                            break;
                        case ThrowResult.Scissors:
                            winner = Winner.Player;
                            break;
                        default:
                            break;
                    }
                    break;
                case ThrowResult.Scissors:
                    switch (player)
                    {
                        case ThrowResult.Rock:
                            winner = Winner.Player;
                            break;
                        case ThrowResult.Paper:
                            winner = Winner.Bot;
                            break;
                        case ThrowResult.Scissors:
                            winner = Winner.Tie;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }

            if (!EnumHelper.EnumValueIsValid(winner))
            {
                throw new InvalidOperationException("Winner value not valid");
            }

            return winner;
        }
    }
}
