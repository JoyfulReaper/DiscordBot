using DiscordBotLibrary.Models;
using DiscordBotLibrary.Repositories.Interfaces;
using DiscordBotLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Services
{
    public class WarningService : IWarningService
    {
        private readonly IWarningRepository _warningRepository;

        public WarningService(IWarningRepository warningRepository)
        {
            _warningRepository = warningRepository;
        }

        public Task ClearWarningsAsync(long id, long guildId)
        {
            return _warningRepository.ClearWarningsAsync(id, guildId);
        }

        public Task<IEnumerable<Warning>> GetWarningsAsync(long userId, long guildId)
        { 
            return _warningRepository.GetWarningsAsync(userId, guildId);
        }

        public Task AddWarningAsync(Warning warning)
        {
            return _warningRepository.AddWarningAsync(warning);
        }
        public Task<WarningAction?> GetWarningActionAsync(long guildId)
        { 
            return _warningRepository.GetWarningActionAsync(guildId);
        }
    }
}
