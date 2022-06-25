using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Attributes
{
    public class MinValueAttribute : ParameterPreconditionAttribute
    {
        private readonly int _minValue;

        public MinValueAttribute(int minValue)
        {
            _minValue = minValue;
        }



        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, IParameterInfo parameterInfo, object value, IServiceProvider services)
        {
            var intString = value as string;
            if (intString == null)
            {
                throw new Exception("Value Cannot be cast to a string");
            }

            if (int.TryParse(intString, out int result))
            {
                if (_minValue > result)
                {
                    return Task.FromResult(PreconditionResult.FromError($"Parameter: '{parameterInfo.Name}' is too small. Min value is {_minValue}"));
                }
            }

            return Task.FromResult(PreconditionResult.FromSuccess());

        }
    }
}
