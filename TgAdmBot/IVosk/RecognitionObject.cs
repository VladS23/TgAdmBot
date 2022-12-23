using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgAdmBot.IVosk
{
    internal class RecognitionObject
    {
        public Database.Chat chat;
        public Telegram.Bot.Types.Message voiceMessage;
    }
}
