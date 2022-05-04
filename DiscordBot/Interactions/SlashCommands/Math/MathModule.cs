using Discord.Interactions;
using DiscordBotLibrary.Helpers;
using System.Data;

namespace DiscordBot.Interactions.SlashCommands.Math
{
    public class MathModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("math", "Do math!")]
        public async Task DoMath(string math)
        {
            var dt = new DataTable();

            try
            {
                var result = dt.Compute(math, null);

                await RespondAsync("", EmbedHelper.GetEmbedAsArray("Math", $"Math: `{math}`\nResult: `{result}`",
                    thumbImage: ImageLookup.GetImageUrl(nameof(ImageLookup.MATH_IMAGES))));

            }
            catch (EvaluateException)
            {
                await RespondAsync("Unable to evaluate");
            }
            catch (SyntaxErrorException)
            {
                await RespondAsync("Syntax error");
            }

        }
    }
}
