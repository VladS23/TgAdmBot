using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TgAdmBot.Database;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {
        private async Task HandleVoiceMessage(CancellationToken cancellationToken, Telegram.Bot.Types.Message message)
        {
            Task.Run(() => { 
            //TODO tts
            });
        }
    }

}

