using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Attributes
{
    public class MaxValueAttribute : ParameterPreconditionAttribute
    {
        private readonly int _maxValue;

        public MaxValueAttribute(int maxValue)
        {
            _maxValue = maxValue;
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
                if (_maxValue < result)
                {
                    return Task.FromResult(PreconditionResult.FromError($"Parameter: '{parameterInfo.Name}' is too large. Max value is {_maxValue}"));
                }
            }
            
            return Task.FromResult(PreconditionResult.FromSuccess());


        }
    }
}
