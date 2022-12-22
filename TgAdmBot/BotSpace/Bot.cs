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
        internal ITelegramBotClient botClient = new TelegramBotClient(Program.botToken);
        internal ReceiverOptions receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { },
        };
        internal CancellationToken cancellationToken = new CancellationTokenSource().Token;

        public Bot() {
            BotDatabase db = new BotDatabase();
            
            botClient.StartReceiving(
                this.HandleUpdateAsync,
                this.HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
        }

    }

}

