using Telegram.Bot;
using Telegram.Bot.Polling;
using TgAdmBot.Database;
using TgAdmBot.Logger;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {
        public static ITelegramBotClient currentObject;
        internal ITelegramBotClient botClient = new TelegramBotClient(Program.botToken);
        internal ReceiverOptions receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { },
        };
        internal CancellationToken cancellationToken = new CancellationTokenSource().Token;


        public Bot()
        {
            BotDatabase db = new BotDatabase();
            currentObject = botClient;
            botClient.StartReceiving(
                this.HandleUpdateAsync,
                this.HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            new Log("App started", LogType.info);
        }

    }

}

