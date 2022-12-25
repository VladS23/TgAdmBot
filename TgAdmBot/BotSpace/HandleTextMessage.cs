using Telegram.Bot;
using TgAdmBot.Database;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {

        private async Task HandleTextMessage(Telegram.Bot.Types.Message message, Database.User user, Database.Chat chat)
        {
            Database.User? replUser = chat.Users.SingleOrDefault(u => u.TelegramUserId == message.ReplyToMessage?.From?.Id);
            if (replUser == null && message.ReplyToMessage != null)
            {
                replUser = Database.User.GetOrCreate(chat, message.ReplyToMessage.From);
            }





            if (message.Text.Contains("@all"))
            {
                botClient.SendTextMessageAsync(message.Chat, chat.GetAllMentions(), Telegram.Bot.Types.Enums.ParseMode.Markdown);
            }


            switch (message.Text.ToLower().Replace($"@{botClient.GetMeAsync().Result.Username!}", "").Split()[0])
            {
                case "/stt":
                    if (replUser != null)
                    {
                        VoiceMessage? voiseMessage = BotDatabase.db.VoiceMessages.SingleOrDefault(vm => vm.Chat.ChatId == chat.ChatId && vm.MessageId == message.ReplyToMessage.MessageId);
                        if (voiseMessage == null)
                        {
                            botClient.SendTextMessageAsync(chatId: message.Chat, "Сообщения, отправленные раньше добавления бота не будут обработаны.");
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(chatId: message.Chat, voiseMessage.recognizedText, replyToMessageId: message.ReplyToMessage.MessageId);
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Ответьте этой командой на голосовое сообщение.");
                    }
                    break;
                case "/db":
                    if (message.Chat.Id==Convert.ToInt64(Program.ownerId))
                    {
                     //TODO отправка бд в чат   
                    }
                    break;
                case "/help":
                    botClient.SendTextMessageAsync(message.Chat, "" +
                        "Выберите какой раздел команд вас интересует \n" +
                        "1. Развлечения \n" +
                        "2. Настройка беседы\n" +
                        "3. Администрирование");
                    break;
                case "1":
                    if (user.LastMessage == "/help")
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Список развлекательных команд\n" +
                            "1. /nick + имя - установит вам в качестве ника \"имя\"\n" +
                            "2. /nicks - выведет список всех участников беседы \n" +
                            "3. /stat - выведет вашу статистику, пользователи с рангом модератор и выше могут посмотреть статистику пользователей с рангом меньше, чем у них, если напишут это сообщение в ответ на сообщение пользователя, статистику которого необходимо просмотреть\n" +
                            "4. /rnd число1-число2 - сгенерирует случайное число из указанного промежутка\n" +
                            "5. /chs вариант1 или вариант2 - выберет один из указанных вариантов\n" +
                            "6. /me действие - выведет сообщение вида: \"пользователь1 действие пользователь2\" (пользователь2 указывается путем ответа на его сообщение) пользователя указывать необязательно\n" +
                            "7. /wh + действие - выведет: *Случайный участник беседы* действие\n" +
                            "8. /prob + событие - предположит вероятность какого-то события\n" +
                            "9. /chatstat выведет статистику чата");
                    }
                    break;
                case "2":
                    if (user.LastMessage == "/help")
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Список доступных настроек беседы\n" +
                            "1. /setdefaultadmins - назначит администраторов беседы в соответсвтвие с тем, как расставлены права в телеграм, может использоваться только создателем беседы\n" +
                            "2. /voicemessange заблокирует или разблокирует голосовые и видеосообщения, по умолчанию разблокировано, может применяться администраторами или создателем\n" +
                            "3. /setwarninglimitaction - установка наказания за превышение количества предупреждений, по умолчанию mute\n" +
                            "4. /setrules - установка правил");
                    }
                    break;
                case "3":
                    if (user.LastMessage == "/help")
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Список доступных административных действий:\n" +
                            "1. Назначения ранга пользователям. Ранг создателя сразу выдается создателем беседы, остальные ранги могут быть назначены пользователем с рангом выше. Чтобы назначить ранг необходимо написать одну из следующих команд в ответ на сообщение пользователя, которому необходимо назначить ранг\n   /admin\n    /moder\n    /helper\n    /normal\n" +
                            "2. /mute Если ваш ранг выше или равен модератору и выше, чем у пользователя, на сообщение, которого вы ответили, то вы запретите или разрешите ему писать сообщения, не работает на пользователей, которым в настройках беседы телеграм установлен ранг администратор\n" +
                            "3. /muted Выведет список всех, кому запрещено писать сообщения\n" +
                            "4. /ban Исключит пользователя из беседы и добавит его в черный список чата, данная команда доступна только создателю и администратора, не работает на пользователей, котрым в настройках беседы в телеграм установлен ранг администратора\n" +
                            "5. /unban  Исключит пользователя из черного списка чата, команда доступна администраторам и создателям\n" +
                            "6. /warn Выдаст пользователю предупреждение, по достижению трех предупреждений он будет исключен из беседы, команда доступна создателю или администраторам\n" +
                            "7. /warns выведет список предупрежденных пользователей беседы, доступна создателю или администраторам \n" +
                            "8. /unwarn снимет с пользователя все предупреждения");
                    }
                    break;
                case "/chatstat":

                    BotDatabase.db.SaveChanges();
                    botClient.SendTextMessageAsync(message.Chat, chat.GetInfo());
                    break;
                case "/setdefaultadmins":
                    if (user.UserRights == UserRights.creator)
                    {
                        BotDatabase.db.SaveChanges();
                        botClient.SendTextMessageAsync(message.Chat, chat.SetDefaultAdmins());
                        break;
                    }
                    break;
                case "/rules":

                    BotDatabase.db.SaveChanges();
                    botClient.SendTextMessageAsync(message.Chat, $"Правила чата:\n{chat.Rules}");
                    break;
                case "/setrules":

                    BotDatabase.db.SaveChanges();
                    if (user.UserRights < UserRights.helper)
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Следующим сообщением пришлите правила");
                        break;
                    }
                    break;
                case "/admin":
                    if (user.UserRights < UserRights.administrator)
                    {

                        if (replUser != null)
                        {
                            BotDatabase.db.SaveChanges();
                            if (user.UserRights < replUser.UserRights)
                            {
                                replUser.UserRights = UserRights.administrator;
                                BotDatabase.db.SaveChanges();
                                botClient.SendTextMessageAsync(message.Chat, "Пользователь теперь администратор");
                                break;
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав");
                                break;
                            }
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя");
                            break;
                        }
                    }
                    break;
                case "/moder":
                    if (user.UserRights < UserRights.moderator)
                    {

                        if (replUser != null)
                        {
                            if (user.UserRights < replUser.UserRights)
                            {
                                replUser.UserRights = UserRights.moderator;
                                BotDatabase.db.SaveChanges();
                                botClient.SendTextMessageAsync(message.Chat, "Пользователь теперь модератор");
                                break;
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав");
                                break;
                            }
                        }

                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя");
                            break;
                        }
                    }
                    break;
                case "/helper":
                    if (user.UserRights < UserRights.helper)
                    {
                        if (replUser != null)
                        {
                            if (user.UserRights < replUser.UserRights)
                            {
                                replUser.UserRights = UserRights.helper;
                                BotDatabase.db.SaveChanges();
                                botClient.SendTextMessageAsync(message.Chat, "Пользователь теперь помощник");
                                break;
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав");
                                break;
                            }
                        }

                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя");
                            break;
                        }
                    }
                    break;
                case "/normal":
                    if (user.UserRights < UserRights.normal)
                    {
                        if (replUser != null)
                        {
                            if (user.UserRights < replUser.UserRights)
                            {
                                replUser.UserRights = UserRights.normal;
                                BotDatabase.db.SaveChanges();
                                botClient.SendTextMessageAsync(message.Chat, "Пользователь теперь нормал");
                                break;
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав");
                                break;
                            }
                        }

                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя");
                            break;
                        }
                    }
                    break;
                case "/mute":
                    if (user.UserRights < UserRights.helper)
                    {
                        if (replUser != null)
                        {
                            if (user.UserRights < replUser.UserRights)
                            {
                                if (replUser.IsMuted)
                                {
                                    replUser.IsMuted = false;
                                    BotDatabase.db.SaveChanges();
                                    replUser.Unmute();
                                    botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{user.Nickname}](tg://user?id={user.TelegramUserId}) разрешил пользователю [{replUser.Nickname}](tg://user?id={replUser.TelegramUserId}) писать сообщения", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    break;
                                }
                                else
                                {
                                    if (message.Text.Split().Length == 2)
                                    {
                                        try
                                        {
                                            int time = Convert.ToInt32(message.Text.Split()[1]);
                                            if (time > 0 && time < 99999)
                                            {
                                                replUser.IsMuted = true;
                                                BotDatabase.db.SaveChanges();
                                                replUser.Mute(time);
                                                botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{user.Nickname}](tg://user?id={user.TelegramUserId}) запретил пользователю [{replUser.Nickname}](tg://user?id={replUser.TelegramUserId}) писать сообщения", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                                break;
                                            }
                                            else
                                            {
                                                botClient.SendTextMessageAsync(message.Chat, "Некорректное время мута (0-99999)", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                                break;
                                            }
                                        }
                                        catch
                                        {
                                            botClient.SendTextMessageAsync(message.Chat,"Первый аргумент должен быть числом" , Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        botClient.SendTextMessageAsync(message.Chat, "Укажите время мута", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        break;
                                    }
                                }
                            }

                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя");
                            break;
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        break;
                    }
                    break;
                case "/stat":
                    if (user.UserRights < UserRights.helper)
                    {
                        if (replUser != null)
                        {
                            if (user.UserRights < replUser.UserRights)
                            {
                                botClient.SendTextMessageAsync(message.Chat, replUser.GetInfo(), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                break;
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав");
                                break;
                            }

                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, user.GetInfo(), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                    }
                    break;
                case "/setwarninglimitaction":
                    botClient.SendTextMessageAsync(message.Chat, chat.SetWarningLimitAction(message.From.Id, message.Text));
                    break;
                case "/muted":
                    if (user.UserRights < UserRights.helper)
                    {
                        botClient.SendTextMessageAsync(message.Chat, chat.GetMutedUsers(), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        break;
                    }
                    break;
                case "/ban":
                    if (replUser != null)
                    {
                        if (user.UserRights < UserRights.moderator && user.UserRights < replUser.UserRights)
                        {
                            replUser.Ban();
                            botClient.SendTextMessageAsync(message.Chat, $"[{user.Nickname}](tg://user?id={user.TelegramUserId}) забанил [{replUser.Nickname}](tg://user?id={replUser.TelegramUserId}) " +
                                $"чтобы вернуть данного пользователя обратно администратор или создатель должен написать /unban в ответ на любое сообщение пользователя, а затем пригласить его в чат " +
                                $"или вручную удалить его из черного списка чата, а затем пригласить ", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия",
                            Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать",
                            Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        break;
                    }
                case "/unban":
                    if (replUser != null)
                    {
                        if (user.UserRights < UserRights.moderator && user.UserRights < replUser.UserRights)
                        {

                            replUser.UnBan();
                            botClient.SendTextMessageAsync(message.Chat, $"[{user.Nickname}](tg://user?id={user.TelegramUserId}) разбанил " +
                                $"[{replUser.Nickname}](tg://user?id={replUser.TelegramUserId}) теперь он вновь может быть приглашен в чат",
                                Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия",
                                Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать",
                            Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        break;
                    }
                case "/warn":
                    if (replUser != null)
                    {
                        if (user.UserRights < UserRights.helper && user.UserRights < replUser.UserRights)
                        {
                            //TODO переделать. Будет отображаться неверное число варнов
                            if (replUser.WarnsCount != chat.WarnsLimit - 1)
                            {
                                replUser.Warn();
                                if (message.Text.Length > 6)
                                {
                                    botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{user.Nickname}](tg://user?id={user.TelegramUserId}) выдал предупреждение пользователю [{user.Nickname}](tg://user?id={user.TelegramUserId})\nПо причине:{message.Text.Substring(5)}\nПредупреждений до наказания {chat.WarnsLimit - replUser.WarnsCount}", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    break;
                                }
                                else
                                {
                                    botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{user.Nickname}](tg://user?id={user.TelegramUserId}) выдал предупреждение пользователю [{replUser.Nickname}](tg://user?id={replUser.TelegramUserId})\nПредупреждений до наказания {chat.WarnsLimit - replUser.WarnsCount}", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    break;
                                }
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, replUser.WarningLimitAction());
                            }

                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        break;
                    }
                    break;
                case "/unwarn":
                    if (replUser != null)
                    {
                        if (user.UserRights < UserRights.helper && user.UserRights < replUser.UserRights)
                        {
                            replUser.WarnsCount = 0;
                            BotDatabase.db.SaveChanges();
                            botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{user.Nickname}](tg://user?id={user.TelegramUserId}) снял все предупреждения с пользователя " +
                                $"[{replUser.Nickname}](tg://user?id={replUser.TelegramUserId})",
                                Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;

                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия",
                                Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать",
                            Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        break;
                    }
                case "/warns":
                    if (user.UserRights < UserRights.helper)
                    {
                        botClient.SendTextMessageAsync(message.Chat, chat.GetWarnedUsers(),
                            Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        break;
                    }
                    break;
                case "/voicemessage":
                    if (user.UserRights < UserRights.moderator)
                    {
                        if (!chat.VoiceMessagesDisallowed)
                        {
                            chat.VoiceMessagesDisallowed = true;
                            botClient.SendTextMessageAsync(message.Chat, "Теперь в данной беседе запрещены голосовые сообщения");
                        }
                        else
                        {
                            chat.VoiceMessagesDisallowed = false;
                            botClient.SendTextMessageAsync(message.Chat, "Теперь в данной беседе разрешены голосовые сообщения");
                        }
                        break;
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав для выполнения данной команды");
                        break;
                    }
                case "/nick":
                    if (message.Text.Length > 8)
                    {
                        if (message.Text.Length < 30)
                        {
                            try
                            {
                                user.Nickname = message.Text.Substring(6);
                                BotDatabase.db.SaveChanges();
                                botClient.SendTextMessageAsync(message.Chat, $"Вам установлен ник [{user.Nickname}](tg://user?id={user.TelegramUserId})", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            }
                            catch
                            {
                                botClient.SendTextMessageAsync(message.Chat, $"Использованы недопустимые символы", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            }
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, $"Ник слишком длинный", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, $"Ник слишком короткий", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                    break;
                case "/nicks":
                    botClient.SendTextMessageAsync(message.Chat, user.Chat.GetChatNicknames(), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    break;
                case "/rnd":
                    if (message.Text.Length > 6)
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotGames.GetRandomNumber(message.Text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                    break;
                case "/chs":
                    if (message.Text.Length > 11)
                    {
                        user.LastMessage = message.Text;
                        BotDatabase.db.SaveChanges();
                        botClient.SendTextMessageAsync(message.Chat, BotGames.Chose(message.Text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Сообщение слишком короткое", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                    break;
                case "/me":
                    if (message.Text.Length > 4)
                    {
                        if (message.ReplyToMessage != null)
                        {
                            string mestext = message.Text.Substring(4);
                            botClient.SendTextMessageAsync(message.Chat, $"[{user.Nickname}](tg://user?id={user.TelegramUserId}) " + mestext + $" [{replUser.Nickname}](tg://user?id={replUser.TelegramUserId})", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);

                        }
                        else
                        {
                            string mestext = message.Text.Substring(4);
                            botClient.SendTextMessageAsync(message.Chat, $"[{user.Nickname}](tg://user?id={user.TelegramUserId}) " + mestext, Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        }
                    }
                    break;
                case "/wh":
                    if (message.Text.Length > 4)
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotGames.Who(message.Text, user.Chat), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Сообщение слишком короткое", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                    break;
                case "/prob":
                    if (message.Text.Length > 5)
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotGames.Probability(message.Text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Сообщение слишком короткое", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                    break;
                default:
                    switch (user.LastMessage)
                    {
                        case "/setrules":
                            if (user.UserRights < UserRights.moderator)
                            {
                                if (message.Text.Length > 15)
                                {
                                    if (message.Text.Length < 10000)
                                    {

                                        string rules = message.Text.Substring(9);
                                        chat.Rules = rules;
                                        botClient.SendTextMessageAsync(message.Chat, $"Правила чата установлены", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    }
                                    else
                                    {
                                        botClient.SendTextMessageAsync(message.Chat, "Правила слишком длинные", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    }
                                }
                                else
                                {
                                    botClient.SendTextMessageAsync(message.Chat, "Правила слишком короткие", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                }
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, "Только админы и владелец могут изменять правила", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            }
                            break;
                        default:
                            break;
                    }
                    break;

            }

            user.UpdateStatistic(message);
        }
    }
}
