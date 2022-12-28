using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TgAdmBot.Database;
using TgAdmBot.Logger;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {
        private async Task HandleCallbackAsync(Telegram.Bot.Types.Update update)
        {
            Telegram.Bot.Types.CallbackQuery query = update.CallbackQuery;
            if (query.Data.StartsWith("/prvt!"))
            {
                PrivateMessage? prvtMsg = BotDatabase.db.PrivateMessages.SingleOrDefault(p => p.Callback == query.Data);
                if (prvtMsg != null)
                {
                    Chat chat = BotDatabase.db.Chats.Single(c => c.TelegramChatId == query.Message.Chat.Id);
                    long userId = chat.Users.Single(u => u.TelegramUserId == query.From.Id).TelegramUserId;
                    if (prvtMsg.Users.Where(u => u.TelegramUserId == userId).ToList().Count > 0 && prvtMsg.Mode == PrivateMessageModes.allow)
                    {
                        //allowed (allow)
                        botClient.AnswerCallbackQueryAsync(query.Id, prvtMsg.Text, true);
                    }
                    else if (prvtMsg.Users.Where(u => u.TelegramUserId == userId).ToList().Count == 0 && prvtMsg.Mode == PrivateMessageModes.disallow)
                    {
                        //allowed (disallow)
                        botClient.AnswerCallbackQueryAsync(query.Id, prvtMsg.Text, true);
                    }
                    else
                    {
                        botClient.AnswerCallbackQueryAsync(query.Id, "Вам недоступен просмотр данного сообщения!", true);
                    }
                }
                else
                {
                    botClient.AnswerCallbackQueryAsync(query.Id, "Извините, сообщение было утеряно(", true);
                }
            }
            else
            {
                botClient.AnswerCallbackQueryAsync(query.Id, "Извините, сообщение было утеряно(", true);
            }
        }

    }
}
