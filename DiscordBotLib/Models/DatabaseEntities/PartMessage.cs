﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLib.Models.DatabaseEntities
{
    public class PartMessage : DatabaseEntity
    {
        public ulong ServerId { get; set; }
        public String Message { get; set; }
    }
}
