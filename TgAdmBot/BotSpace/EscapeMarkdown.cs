using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TgAdmBot.Database;
using TgAdmBot.Logger;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {
        public static string EscapeMarkdown(string str)
        {
            string nameMd = Regex.Replace(str, @"([\\|*_{}[\]()#+\-.!`])", @"\$1");
            return nameMd;
        }

    }

}

