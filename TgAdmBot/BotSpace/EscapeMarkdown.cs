using System.Text.RegularExpressions;

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

