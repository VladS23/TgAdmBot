using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TgAdmBot.Database;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {
        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            //Servise output
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                #region Подготовка к обработке сообщения
                Telegram.Bot.Types.Message message = update.Message!;

                Database.Chat chat = Database.Chat.GetOrCreate(message);

                Database.User user = Database.User.GetOrCreate(chat, message.From);
                #endregion
                #region Авторазмут пользователей
                if (DateTime.Now > Program.LastDbChech.AddMinutes(1))
                {
                    Program.LastDbChech=DateTime.Now;
                    Console.WriteLine("DbChecked");
                    foreach (Database.Chat ch in BotDatabase.db.Chats)
                    {
                        foreach (Database.User us in ch.Users.Where(us => us.IsMuted && us.UnmuteTime < DateTime.Now).ToList())
                        {
                            us.Unmute();
                        }
                    }
                }
                #endregion
                chat.MessagesCount += 1;
                BotDatabase.db.SaveChanges();


                if (message.Text != null)
                {
                    Task handledTextTask = this.HandleTextMessage(message, user, chat);
                }
                else if (message.Voice!=null)
                {
                    this.HandleVoiceMessage(message, user, chat);
                }
                else
                {
                    user.UpdateStatistic(message);
                }
            }
        }

    }
}
