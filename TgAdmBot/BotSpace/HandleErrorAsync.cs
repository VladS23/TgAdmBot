using Telegram.Bot;
using TgAdmBot.Logger;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {
        private async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            new Log(Newtonsoft.Json.JsonConvert.SerializeObject(exception), LogType.error);
        }
    }
}
