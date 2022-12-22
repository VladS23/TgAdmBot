using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TgAdmBot.BotSpace;
using TgAdmBot.Database;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {

        private async Task HandleTextMessage(Telegram.Bot.Types.Message message, Database.User user, Database.Chat chat)
        {
            Database.User? replUser = null;
            if (message.ReplyToMessage != null)
            {
                Database.User? replyUser = chat.Users.SingleOrDefault(u => u.TelegramUserId == message.ReplyToMessage.From.Id);
                if (replyUser == null)
                {
                    replyUser = Database.User.GetOrCreate(chat, message);
                }
            }


            switch (message.Text.ToLower().Replace($"@{botClient.GetMeAsync().Result.Username!}", "").Split()[0])
            {
                case "/stt":
                    break;
                case "/help":
                    await botClient.SendTextMessageAsync(message.Chat, "" +
                        "Выберите какой раздел команд вас интересует \n" +
                        "1. Развлечения \n" +
                        "2. Настройка беседы\n" +
                        "3. Администрирование");
                    break;
                case "1":
                    if (user.LastMessage == "/help")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Список развлекательных команд\n" +
                            "1. Ник + имя - установит вам в качестве ника \"имя\"\n" +
                            "2. Участники - выведет список всех участников беседы \n" +
                            "3. Стата - выведет вашу статистику, пользователи с рангом модератор и выше могут посмотреть статистику пользователей с рангом меньше, чем у них, если напишут это сообщение в ответ на сообщение пользователя, статистику которого необходимо просмотреть\n" +
                            "4. Рнд число1-число2 - сгенерирует случайное число из указанного промежутка\n" +
                            "5. Вбр вариант1 или вариант2 - выберет один из указанных вариантов\n" +
                            "6. Me действие - выведет сообщение вида: \"пользователь1 действие пользователь2\" (пользователь2 указывается путем ответа на его сообщение)\n" +
                            "7. кт + действие - выведет: *Случайный участник беседы* действие\n" +
                            "8. Вртн + событие - предположит вероятность какого-то события");
                    }
                    break;
                case "2":
                    if (user.LastMessage == "/help")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Список доступных настроек беседы\n" +
                            "1. /setdefaultadmins - назначит администраторов беседы в соответсвтвие с тем, как расставлены права в телеграм, может использоваться только создателем беседы\n" +
                            "2. /voicemessange заблокирует или разблокирует голосовые и видеосообщения, по умолчанию разблокировано, может применяться администраторами или создателем\n" +
                            "3. /setwarninglimitaction - установка наказания за превышение количества предупреждений, по умолчанию mute\n" +
                            "4. /setrules - установка правил");
                    }
                    break;
                case "3":
                    if (user.LastMessage == "/help")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Список доступных административных действий:\n" +
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
                    await botClient.SendTextMessageAsync(message.Chat, chat.GetInfo());
                    break;
                case "/setdefaultadmins":
                    if (user.UserRights == UserRights.creator)
                    {

                        BotDatabase.db.SaveChanges();
                        await botClient.SendTextMessageAsync(message.Chat, chat.SetDefaultAdmins());
                        break;
                    }
                    break;
                case "/rules":

                    BotDatabase.db.SaveChanges();
                    await botClient.SendTextMessageAsync(message.Chat, $"Правила чата:\n{chat.Rules}");
                    break;
                case "/setrules":

                    BotDatabase.db.SaveChanges();
                    if (user.UserRights < UserRights.helper)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Следующим сообщением пришлите правила");
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
                                await botClient.SendTextMessageAsync(message.Chat, "Пользователь теперь администратор");
                                break;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав");
                                break;
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя");
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
                                await botClient.SendTextMessageAsync(message.Chat, "Пользователь теперь модератор");
                                break;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав");
                                break;
                            }
                        }

                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя");
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
                                await botClient.SendTextMessageAsync(message.Chat, "Пользователь теперь помощник");
                                break;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав");
                                break;
                            }
                        }

                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя");
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
                                await botClient.SendTextMessageAsync(message.Chat, "Пользователь теперь нормал");
                                break;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав");
                                break;
                            }
                        }

                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя");
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
                                    await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{user.Nickname}](tg://user?id={user.TelegramUserId}) разрешил пользователю [{replUser.Nickname}](tg://user?id={replUser.TelegramUserId}) писать сообщения", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    break;
                                }
                                else
                                {
                                    replUser.IsMuted = true;
                                    BotDatabase.db.SaveChanges();
                                    replUser.Mute();
                                    await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{user.Nickname}](tg://user?id={user.TelegramUserId}) запретил пользователю [{replUser.Nickname}](tg://user?id={replUser.TelegramUserId}) писать сообщения", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    break;
                                }
                            }

                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя");
                            break;
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
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
                                await botClient.SendTextMessageAsync(message.Chat, replUser.GetInfo(), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                break;
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав");
                                break;
                            }

                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, user.GetInfo(), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                    }
                    break;
                case "/setwarninglimitaction":
                    await botClient.SendTextMessageAsync(message.Chat, chat.SetWarningLimitAction(message.From.Id, message.Text));
                    break;
                case "/muted":
                    if (user.UserRights < UserRights.helper)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, chat.GetMutedUsers(), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        break;
                    }
                    break;
                case "/ban":
                    if (replUser != null)
                    {
                        if (user.UserRights < UserRights.moderator && user.UserRights < replUser.UserRights)
                        {
                            replUser.Ban();
                            await botClient.SendTextMessageAsync(message.Chat, $"[{user.Nickname}](tg://user?id={user.TelegramUserId}) забанил [{replUser.Nickname}](tg://user?id={replUser.TelegramUserId}) " +
                                $"чтобы вернуть данного пользователя обратно администратор или создатель должен написать /unban в ответ на любое сообщение пользователя, а затем пригласить его в чат " +
                                $"или вручную удалить его из черного списка чата, а затем пригласить ", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия",
                            Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать",
                            Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        break;
                    }
                case "/unban":
                    if (replUser != null)
                    {
                        if (user.UserRights < UserRights.moderator && user.UserRights < replUser.UserRights)
                        {

                            replUser.UnBan();
                            await botClient.SendTextMessageAsync(message.Chat, $"[{user.Nickname}](tg://user?id={user.TelegramUserId}) разбанил " +
                                $"[{replUser.Nickname}](tg://user?id={replUser.TelegramUserId}) теперь он вновь может быть приглашен в чат",
                                Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия",
                                Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать",
                            Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        break;
                    }
                case "/warn":
                    if (replUser != null)
                    {
                        if (user.UserRights < UserRights.helper && user.UserRights < replUser.UserRights)
                        {
                            if (replUser.WarnsCount != chat.WarnsLimit - 1)
                            {
                                replUser.Warn();
                                if (message.Text.Length > 6)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{user.Nickname}](tg://user?id={user.TelegramUserId}) выдал предупреждение пользователю [{user.Nickname}](tg://user?id={user.TelegramUserId})\nПо причине:{message.Text.Substring(5)}\nПредупреждений до наказания {chat.WarnsLimit - replUser.WarnsCount}", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    break;
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{user.Nickname}](tg://user?id={user.TelegramUserId}) выдал предупреждение пользователю [{replUser.Nickname}](tg://user?id={replUser.TelegramUserId})\nПредупреждений до наказания {chat.WarnsLimit - replUser.WarnsCount}", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    break;
                                }
                            }
                            else
                            {
                                await botClient.SendTextMessageAsync(message.Chat, replUser.WarningLimitAction());
                            }

                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать", Telegram.Bot.Types.Enums.ParseMode.Markdown);
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
                            await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{user.Nickname}](tg://user?id={user.TelegramUserId}) снял все предупреждения с пользователя " +
                                $"[{replUser.Nickname}](tg://user?id={replUser.TelegramUserId})",
                                Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;

                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия",
                                Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            break;
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать",
                            Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        break;
                    }
                case "/warns":
                    if (user.UserRights < UserRights.helper)
                    {
                        await botClient.SendTextMessageAsync(message.Chat, chat.GetWarnedUsers(),
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
                            await botClient.SendTextMessageAsync(message.Chat, "Теперь в данной беседе запрещены голосовые сообщения");
                        }
                        else
                        {
                            chat.VoiceMessagesDisallowed = false;
                            await botClient.SendTextMessageAsync(message.Chat, "Теперь в данной беседе разрешены голосовые сообщения");
                        }
                        break;
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав для выполнения данной команды");
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
                                await botClient.SendTextMessageAsync(message.Chat, $"Вам установлен ник [{user.Nickname}](tg://user?id={user.TelegramUserId})", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            }
                            catch
                            {
                                await botClient.SendTextMessageAsync(message.Chat, $"Использованы недопустимые символы", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, $"Ник слишком длинный", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(message.Chat, $"Ник слишком короткий", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                    break;
                case "/nicks":
                    await botClient.SendTextMessageAsync(message.Chat, user.Chat.GetChatNicknames(), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    return;

                default:
                    switch (user.LastMessage)
                    {
                        case "/setrules":
                            if (user.UserRights < UserRights.moderator)
                            {
                                chat.Rules = message.Text;
                                await botClient.SendTextMessageAsync(message.Chat, $"Новые правила:\n{chat.Rules}");
                                BotDatabase.db.SaveChanges();
                            }
                            break;
                        default: break;
                    }
                    break;
            }

            user.UpdateStatistic(message);
        }
    }
}
