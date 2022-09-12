using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using MySql.Data.MySqlClient;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace TgAdmBot
{
    class Program
    {
        //This list help to determine admin hierarсhy
        private static string[] AdmRangs = { "creator", "administrator", "moderator", "helper", "normal" };
        //Initializing botapi and database connection
        private static string botToken = "5328960378:AAH1fskxZcH3GEXMVKHROuhfnNJQsSj8gvU";
        private static ITelegramBotClient bot = new TelegramBotClient(botToken);
        private static MySqlConnection conn = new MySqlConnection("server=localhost; port=3306; username=root; password=root; database=tgadmbot");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //Servise output
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            try
            {
                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    //I use two object to work with the messange,
                    //since each of them does not implement all the functionality of the second one
                    string strupdate = Newtonsoft.Json.JsonConvert.SerializeObject(update);
                    MyMessage mymessage = Newtonsoft.Json.JsonConvert.DeserializeObject<MyMessage>(strupdate);
                    Telegram.Bot.Types.Message message = update.Message;
                    //Initializing the chat
                    if (!isThisChatInDB(message))
                    {
                        CreateThisChatInDb(message);
                        await botClient.SendTextMessageAsync(message.Chat, "Вас приветсвует бот администратор !help для получения списка команд");

                    }
                    //Initializing the user
                    if (!isThisUserInDB(mymessage.message.chat.id, mymessage.message.from.id))
                    {
                        CreateThisUserInDB(mymessage.message.chat.id, mymessage.message.from.id, mymessage.message.from.first_name);
                    }
                    if (mymessage.message.reply_to_message != null)
                    {
                        if (!isThisUserInDB(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id))
                        {
                            CreateThisUserInDB(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id, mymessage.message.reply_to_message.from.first_name);
                        }
                    }
                    //Collecting statistics
                    Analyzer(mymessage, message);
                    if (message.Text != null)
                    {
                        //Processing help commands
                        if (message.Text.ToLower() == "!help")
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Выберите какой раздел команд вас интересует \n 1. Развлечения \n 2. Настройка беседы \n 3. Администрирование");
                            return;
                        }
                        if (message.Text.ToLower() == "развлечения")
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Список развлекательных команд\n1. Ник + имя - установит вам в качестве ника \"имя\"\n2. Ники - выведет список всех ников беседы \n3. Стата - выведет вашу статистику, пользователи с рангом модератор и выше могут посмотреть статистику пользователей с рангом меньше, чем у них, если напишут это сообщение в ответ на сообщение пользователя, статистику которого необходимо просмотреть\n4. Рнд число1-число2 - сгенерирует случайное число из указанного промежутка\n5. Вбр вариант1 или вариант2 - выберет один из указанных вариантов\n6. Me действие - выведет сообщение вида: \"пользователь1 действие пользователь2\" (пользователь2 указывается путем ответа на его сообщение)\n 7. кт + действие - выведет: *Случайный участник беседы* действие\n 8. Вртн + событие - предположит вероятность какого-то события");
                            return;
                        }
                        if (message.Text.ToLower() == "настройки беседы")
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Список доступных настроек беседы\n1. setdefaultadmins - назначит администраторов беседы в соответсвтвие с тем, как расставлены права в телеграм, может использоваться только создателем беседы\n 2. /voicemessange заблокирует или разблокирует голосовые и видеосообщения, по умолчанию разблокировано, может применяться администраторами или создателем");
                            return;
                        }
                        if (message.Text.ToLower() == "администрирование")
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Список доступных административных действий:\n 1. Назначения ранга пользователям. Ранг создателя сразу выдается создателем беседы, остальные ранги могут быть назначены пользователем с рангом выше. Чтобы назначить ранг необходимо написать одну из следующих команд в ответ на сообщение пользователя, которому необходимо назначить ранг\n   /admin\n    /moder\n    /helper\n    /normal\n 2. /mute Если ваш ранг выше или равен модератору и выше, чем у пользователя, на сообщение, которого вы ответили, то вы запретите или разрешите ему писать сообщения, не работает на пользователей, которым в настройках беседы телеграм установлен ранг администратор\n 3. /muted Выведет список всех, кому запрещено писать сообщения\n 4. /ban Исключит пользователя из беседы и добавит его в черный список чата, данная команда доступна только создателю и администратора, не работает на пользователей, котрым в настройках беседы в телеграм установлен ранг администратора\n 5. /unban  Исключит пользователя из черного списка чата, команда доступна администраторам и создателям\n 6. /warn Выдаст пользователю предупреждение, по достижению трех предупреждений он будет исключен из беседы, команда доступна создателю или администраторам\n 7. /warns выведет список предупрежденных пользователей беселы, доступна создателю или администраторам");
                            return;
                        }
                        //This command should be used to search for the chat creator
                        //if it is defined incorrectly during initialization
                        if (message.Text.ToLower() == "setdefaultadmins")
                        {
                            await botClient.SendTextMessageAsync(message.Chat, SetDefaultAdmins(message.Chat.Id, message.From.Id));
                            return;
                        }
                        //Entertainment commands
                        if (message.Text.ToLower() == "актив")
                        {
                            await botClient.SendTextMessageAsync(message.Chat, GetUsersActivity(message.Chat.Id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            return;
                        }
                        if (message.Text.Length > 4)
                        {
                            if (message.Text.ToLower()[0] == 'н' && message.Text.ToLower()[1] == 'и' && message.Text.ToLower()[2] == 'к' && message.Text.ToLower()[3] == ' ')
                            {
                                await botClient.SendTextMessageAsync(message.Chat, SetNickname(mymessage.message.chat.id, mymessage.message.from.id, mymessage.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;
                            }
                        }
                        if (message.Text.Length == 4)
                        {
                            if (message.Text.ToLower() == "ники")
                            {
                                await botClient.SendTextMessageAsync(message.Chat, GetChatNicknames(mymessage.message.chat.id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;
                            }
                        }
                        if (message.Text.Length == 5)
                        {
                            if (message.Text.ToLower() == "стата")
                            {
                                if (mymessage.message.reply_to_message == null)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat.Id, GetStatistics(mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                                else
                                {
                                    //Such a check allows you to determine the user's access level as a number
                                    //and compare it with the specified one
                                    //maximum level 0, minimum 4
                                    if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 2 && Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) < Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)))
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, GetStatistics(mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Недостаточно прав на выполнение этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                }

                            }
                        }
                        if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 3)
                        {
                            if (message.Text.Length > 6)
                            {
                                if (message.Text.ToLower()[0] == 'р' && message.Text.ToLower()[1] == 'н' && message.Text.ToLower()[2] == 'д' && message.Text.ToLower()[3] == ' ')
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, GetRandomNumber(mymessage.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.Length > 10)
                            {
                                if (message.Text.ToLower()[0] == 'в' && message.Text.ToLower()[1] == 'б' && message.Text.ToLower()[2] == 'р' && message.Text.ToLower()[3] == ' ')
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, Chose(mymessage.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.Length > 3)
                            {
                                if (message.Text.ToLower()[0] == 'm' && message.Text.ToLower()[1] == 'e' && message.Text.ToLower()[2] == ' ')
                                {
                                    if (mymessage.message.reply_to_message != null)
                                    {
                                        string mestext = mymessage.message.text.Substring(3);
                                        await botClient.SendTextMessageAsync(message.Chat, $"[{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) " + mestext + $" [{GetNickname(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id})", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        await botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                                        return;
                                    }
                                }
                            }
                            if (message.Text.Length > 3)
                            {
                                if (message.Text.ToLower()[0] == 'к' && message.Text.ToLower()[1] == 'т' && message.Text.ToLower()[2] == ' ')
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, Who(mymessage.message.text, message.Chat.Id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.Length > 5)
                            {
                                if (message.Text.ToLower()[0] == 'в' && message.Text.ToLower()[1] == 'р' && message.Text.ToLower()[2] == 'т' && message.Text.ToLower()[3] == 'н' && message.Text.ToLower()[4] == ' ')
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, Probability(mymessage.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                        }
                        //Administrative command
                        if (message.Text.ToLower()[0] == '/')
                        {
                            if (message.Text.ToLower().Trim() == "/admin")
                            {
                                await botClient.SendTextMessageAsync(message.Chat, SetAdminStatus("administrator", mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;
                            }
                            if (message.Text.ToLower().Trim() == "/moder")
                            {
                                await botClient.SendTextMessageAsync(message.Chat, SetAdminStatus("moderator", mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;

                            }
                            if (message.Text.ToLower().Trim() == "/helper")
                            {
                                await botClient.SendTextMessageAsync(message.Chat, SetAdminStatus("helper", mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;

                            }
                            if (message.Text.ToLower().Trim() == "/normal")
                            {
                                await botClient.SendTextMessageAsync(message.Chat, SetAdminStatus("normal", mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;

                            }
                            if (mymessage.message.text.ToLower().Trim() == "/mute")
                            {
                                if (mymessage.message.reply_to_message != null)
                                {
                                    if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 2 && Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) < Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)))
                                    {
                                        if (IsThisUserMute(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id))
                                        {
                                            Unmute(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                            await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) разрешил пользователю [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id}) писать сообщения", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                            return;

                                        }
                                        else
                                        {
                                            Mute(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                            await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) запретил пользователю [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id}) писать сообщения, чтобы вновь разрешить данному пользователю писать введите " + "\"/mute\" в ответ на его сообщения", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя, которому необходимо запретить писать", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (mymessage.message.text.ToLower().Trim() == "/muted")
                            {
                                if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 2)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, GetMutedUsers(mymessage.message.chat.id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (mymessage.message.text.ToLower().Trim() == "/ban")
                            {
                                if (mymessage.message.reply_to_message != null)
                                {
                                    if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 1 && Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) < Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)))
                                    {
                                        Ban(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                        await botClient.SendTextMessageAsync(message.Chat, $"[{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) забанил [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id}) чтобы вернуть данного пользователя обратно администратор или создатель должен написать /unban в ответ на любое сообщение пользователя, а затем пригласить его в чат или вручную удалить его из черного списка чата, а затем пригласить ", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (mymessage.message.text.ToLower().Trim() == "/unban")
                            {
                                if (mymessage.message.reply_to_message != null)
                                {
                                    if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 1 && Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) < Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)))
                                    {
                                        UnBan(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                        await botClient.SendTextMessageAsync(message.Chat, $"[{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) разбанил [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id}) теперь он вновь может быть приглашен в чат", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.ToLower().Trim() == "/warn")
                            {
                                if (mymessage.message.reply_to_message != null)
                                {
                                    if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 1 && Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) < Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)))
                                    {
                                        if (GetWarnsCount(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id) + 1 < 3)
                                        {
                                            Warn(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                            await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) выдал предупреждение пользователю [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id}) предупреждений до исключения {GetWarnsCount(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)}/3", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                            return;
                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id}) был исключен за превышение количества предупреждений", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                            Ban(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                            return;
                                        }

                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.ToLower().Trim() == "/unwarn")
                            {
                                if (mymessage.message.reply_to_message != null)
                                {
                                    if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 1 && Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) < Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)))
                                    {
                                        Unwarn(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                        await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) снял все предупреждения с пользователя [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id})", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;

                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (mymessage.message.text.ToLower().Trim() == "/warns")
                            {
                                if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 2)
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, GetWarnedUsers(mymessage.message.chat.id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.ToLower().Trim() == "/voicemessange")
                            {
                                if (Array.IndexOf(AdmRangs, AdminStatus(message.Chat.Id, message.From.Id)) <= 1)
                                {
                                    voiceMessangeCommand(message);
                                    if (isVoiceMessengeBlocked(message) == true)
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "Теперь в данной беседе запрещены голосовые сообщения");
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "Теперь в данной беседе разрешены голосовые сообщения");
                                    }
                                    return;
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав для выполнения данной команды");
                                    return;
                                }

                            }
                        }
                    }
                    //Deletes voice messages if they are blocked
                    if ((message.Voice != null || message.VideoNote != null) & isVoiceMessengeBlocked(message))
                    {
                        await botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                        return;
                    }
                }
            }
            catch
            {
                Console.WriteLine("========================================================\n Неизвестная ошибка при обработке сообщения\n========================================================");
                return;
            }
        }
        //======================
        //entertainment messages
        //======================
        //**************
        //Nicknames Commands
        //**************
        //Сreates a message about all user nicknames
        private static string GetChatNicknames(long chatid)
        {
            //Get all the nicknames and ids of the chat users
            string sql = $"SELECT `Nickname`, `User_ID` FROM `users` WHERE `Chat_id`={chatid}";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            //Create result string
            string result = "Ники беседы: \n";
            int cnt = 1;
            while (reader.Read())
            {
                result = result + $"{cnt}. [{reader[0].ToString()}](tg://user?id={reader[1].ToString()})" + "\n";
                cnt = cnt + 1;
            }
            reader.Close();
            return result;
        }
        //Set user's Nickname
        private static string SetNickname(long chatid, long userid, string messagetext)
        {
            //Checking the validity of the nickname
            if (messagetext.Length > 5)
            {
                if (messagetext.Length < 25)
                {
                    try
                    {
                        if (!messagetext.ToLower().Contains("drop"))
                        {
                            //Process the string and write it to the database
                            string nickname = messagetext.Substring(4);
                            string sql = $"UPDATE `users` SET `Nickname` = '{Regex.Escape(nickname)}' WHERE `users`.`Id` = {chatid.ToString() + userid.ToString()};";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            int rowCount = cmd.ExecuteNonQuery();
                            return $"Вам установлен ник \"[{nickname}](tg://user?id={userid})\", теперь бот будет использовать его при взаимодействии с вами";
                        }
                        else
                        {
                            return "использованы недопустимые символы";
                        }
                    }
                    catch
                    {
                        return "использованы недопустимые символы";
                    }
                }
                else
                {
                    return "Ник слишком длинный";
                }
            }
            else
            {
                return "Ник слишком короткий";
            }
        }
        //Buld message about random user from chat
        private static string Who(string text, long chatid)
        {
            //Counting the number of users in the chat
            string sql = "SELECT COUNT(*) FROM users WHERE Chat_id =" + chatid.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            string sreaderLen = "";
            while (reader.Read())
            {
                sreaderLen = reader.GetString(0);
            }
            int readerLen = Convert.ToInt32(sreaderLen);
            reader.Close();
            //Get a list of users with nicknames
            sql = $"SELECT `Nickname`, `User_ID`  FROM `users` WHERE `Chat_id`={chatid}";
            cmd = new MySqlCommand(sql, conn);
            reader = cmd.ExecuteReader();
            string[][] users = new string[readerLen][];
            int i = 0;
            while (reader.Read())
            {
                users[i] = new string[] { reader[0].ToString(), reader[1].ToString() };
                i = i + 1;
            }
            reader.Close();
            //Process string and build the result
            text = text.Substring(3);
            Random rnd = new Random();
            int curUser = rnd.Next(users.Length-1);
            string result = $"[{ users[curUser][0]}](tg://user?id={users[curUser][1]}) {text}";
            return result;
        }
        //Build message about users activity
        private static string GetUsersActivity(long chatid)
        {
            //Get a list of users their nicknames and the dates of their last activity
            string sql = $"SELECT `Nickname`, `User_ID`,`LastActivity`  FROM `users` WHERE `Chat_id`={chatid}";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            //Begin preparing result's string
            string result = "Активность пользователей: \n";
            int cnt = 1;
            while (reader.Read())
            {
                //Check LastActivity validity
                DateTime lastActivity = new DateTime();
                if (DateTime.TryParse(reader[2].ToString(), out lastActivity))
                {
                        result = result + $"{cnt}. [{reader[0].ToString()}](tg://user?id={reader[1].ToString()}) " + "неактивен "+ (DateTime.Now - DateTime.Parse(reader[2].ToString())).Days +" дней"+"\n";
                        cnt = cnt + 1;
                }
            }
            reader.Close();
            return result;
        }
        //Build user's statistics message
        private static string GetStatistics(MyMessage mymessage)
        {
            //if the message is a response, then we work with the original message
            long chatid = 0;
            long userid = 0;
            string username = "";
            string tgusername = "";
            if (mymessage.message.reply_to_message == null)
            {
                chatid = mymessage.message.chat.id;
                userid = mymessage.message.from.id;
                username = mymessage.message.from.first_name;
                tgusername = mymessage.message.from.username;
            }
            else
            {
                chatid = mymessage.message.reply_to_message.chat.id;
                userid = mymessage.message.reply_to_message.from.id;
                username = mymessage.message.reply_to_message.from.first_name;
                tgusername = mymessage.message.reply_to_message.from.username;
            }
            string info = "";
            //Preparing result's string
            try
            {
                info = "📈 Информация о " + username + "\n";
                try
                {
                    info = info + "👤 Имя: " + tgusername+"\n";
                }
                catch
                {

                }
                info = info + $"👥 Ник : [{GetNickname(chatid, userid)}](tg://user?id={userid})" + "\n";
                info = info + "👑 Ранг: " + AdminStatus(chatid, userid)+"\n";
                if (IsThisUserMute(chatid, userid))
                {
                    info = info + "🚫 Запрещено писать сообщения\n";
                }
                else
                {
                    info = info + "👨‍💻 Разрешено писать сообщения\n";
                }
                info = info + "🕒 Время последней активности: " + GetLastActivity(chatid, userid)+"\n";
                info = info + "⛔️ Количество предупреждений " + GetWarnsCount(chatid, userid) + "/3\n";
                info = info + "✉️ Отправлено сообщений: " + GetMessageCount(chatid, userid) + "\n";
                info = info + "🎤 Отправлено голосовых сообщений: " + GetVoiceMessageCount(chatid, userid) + "\n";
                info = info + "😄 Отправлено стикеров: " + GetStickerCount(chatid, userid) + "\n";
            }
            catch
            {
                info = "Возникла неожиданная ошибка";
            }
            return info;
        }
        //Build probability message
        private static string Probability(string messagetext)
        {
            //Process the string and create a result based on it
            string mes = messagetext.Substring(4);
            Random rnd = new Random();
            return "Вероятность" + mes + $" {rnd.Next(0, 101)}%";
        }
        //Build choose message
        private static string Chose(string messagetext)
        {
            try
            {
                //Process the string and create a result based on it
                string mes = messagetext.Substring(4);
                string[] separator = { " или " };
                string[] variables = mes.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                Random rnd = new Random();
                return "✨✨ Я выбираю " + variables[rnd.Next(0, 2)];
            }
            catch
            {
                return "Неправильный синтаксис команды. Пример правильной команды 'вбр вариант 1 или вариант 2'";
            }
        }
        //Build random number message
        private static string GetRandomNumber(string messagetext)
        {
            try
            {
                //Process the string and create a result based on it
                string mes = messagetext.Substring(4);
                string[] nums = mes.Split('-');
                Random rnd =new Random();
                return "🎲🎲 Я бросил кости и выпало " + rnd.Next(Convert.ToInt32(nums[0]), Convert.ToInt32(nums[1]));
            }
            catch
            {
                return "Неправильный синтаксис команды. Пример правильной команды 'рнд 1-12'";
            }
        }
        //========================================
        //Administration command
        //========================================
        //**************
        //Warns commands
        //**************
        //Build messange about all warned users
        private static string GetWarnedUsers(long chatid)
        {
            //Get a list of user IDs and nicknames that have warn
            string sql = $"SELECT `Nickname`, `User_ID`,`Warns`  FROM `users` WHERE `Chat_id`={chatid} AND `Warns`!=0";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            //Build result string
            string result = "Предупрежденные пользователи: \n";
            int cnt = 1;
            while (reader.Read())
            {
                result = result + $"{cnt}. [{reader[0].ToString()}](tg://user?id={reader[1].ToString()}) " +reader[2].ToString() +"/3"+ "\n";
                cnt = cnt + 1;
            }
            reader.Close();
            //If warned users are exist than retuln result string else return else string
            if (cnt > 1)
            {
                return result;
            }
            else
            {
                return "нет предупрежденных пользователей";
            }
        }
        //Removes all warnings
        private static void Unwarn(long chatid, long userid)
        {
            //Set warns count 0
            string sql = $"UPDATE `users` SET `Warns` = '{0}' WHERE `users`.`Id` = {chatid.ToString() + userid.ToString()};";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();
        }
        //Adds another warning
        private static void Warn(long chatid, long userid)
        {
            //Increases the number of user warnings by 1
            string sql = $"UPDATE `users` SET `Warns` = '{GetWarnsCount(chatid, userid)+1}' WHERE `users`.`Id` = {chatid.ToString() + userid.ToString()};";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();
        }
        //Builds a message about muted user
        private static string GetMutedUsers(long chatid)
        {
            //Get a list of user IDs and nicknames that is muted
            string sql = $"SELECT `Nickname`, `User_ID` FROM `users` WHERE `Chat_id`={chatid} AND `IsMute`=1";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            //Build result string
            string result = "Пользователи, которым запрещено писать: \n";
            int cnt = 1;
            while (reader.Read())
            {
                result = result + $"{cnt}. [{reader[0].ToString()}](tg://user?id={reader[1].ToString()})" + "\n";
                cnt = cnt + 1;
            }
            reader.Close();
            //If muted users are exist than retuln result string else return else string
            if (cnt >  1)
            {
                return result;
            }
            else
            {
                return "все пользователи чата могут писать";
            }
        }
        //**************
        //Ban commands
        //**************
        //Add user in the ban
        private static void Ban(long chatid, long userid)
        {
            //Send a request to telegram api and ban user
            try
            {
                using (HttpClientHandler hndl = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        string restext = $"https://api.telegram.org/bot{botToken}/banChatMember?user_id={userid}&chat_id={chatid}";
                        using (var request = cln.GetAsync(restext).Result)
                        {

                        }
                    }
                }
            }
            catch
            {

            }
        }
        //Removing a user from the chat blacklist
        private static void UnBan(long chatid, long userid)
        {
            try
            {
                //Send a request to telegram api and ban user
                using (HttpClientHandler hndl = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        string restext = $"https://api.telegram.org/bot{botToken}/unbanChatMember?user_id={userid}&chat_id={chatid}&only_if_banned=true";
                        using (var request = cln.GetAsync(restext).Result)
                        {

                        }
                    }
                }
            }
            catch
            {

            }
        }
        //**************
        //Mute commands
        //**************
        //Prohibit the user from writing messages
        private static void Mute(long chatid, long userid)
        {
            try
            {
                //Send a request to telegram api and mute user
                using (HttpClientHandler hndl = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        string restext = $"https://api.telegram.org/bot{botToken}/restrictChatMember?user_id={userid}&chat_id={chatid}";
                        using (var request = cln.GetAsync(restext).Result)
                        {

                        }
                        //and update database cell
                        string sql = $"Update users set IsMute = 1 where ID = {chatid.ToString() + userid.ToString()}";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        int rowCount = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {

            }

        }
        //Allow the user to write messages
        private static void Unmute(long chatid, long userid)
        {
            try
            {
                //Send a request to telegram api and unmute users
                using (HttpClientHandler hndl = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        string restext = $"https://api.telegram.org/bot{botToken}/restrictChatMember?user_id={userid}&chat_id={chatid}&until_date={((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds()+35}";
                        using (var request = cln.GetAsync(restext).Result)
                        {

                        }
                        //and update database cell
                        string sql = $"Update users set IsMute = 0 where ID = {chatid.ToString() + userid.ToString()}";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        int rowCount = cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {

            }
        }
        //Checks whether the user can write messages
        private static bool IsThisUserMute(long chatid, long userid)
        {
            //request to database
            string sql = "SELECT IsMute FROM users WHERE ID =" + chatid.ToString() + userid.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            bool nickName = (bool)cmd.ExecuteScalar();
            return nickName;
        }
        //**************
        //Admin Assignment Commands
        //**************
        //If all conditions are met, sets the user's rank
        private static string SetAdminStatus(string admlvl, MyMessage mymessage)
        {
            //If the request came from a user with sufficient rank sets the specified rank to the specified user
            if (mymessage.message.reply_to_message != null)
            {
                if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) < Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)))
                {
                    if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) < Array.IndexOf(AdmRangs, admlvl))
                    {
                        try
                        {   
                            string sql = $"UPDATE `users` SET `Admin` = '{admlvl}' WHERE `users`.`ID` = {mymessage.message.chat.id.ToString() + mymessage.message.reply_to_message.from.id.ToString()};";
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            int rowCount = cmd.ExecuteNonQuery();
                            return $"Пользователь [{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) назначил пользователю [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id}) ранг {admlvl}";
                        }
                        catch
                        {
                            return "Произошла неизвестная ошибка";
                        }
                    }
                    else
                    {
                        return "Невозможно установить ранг выше или равный собственному";
                    }
                }
                else
                {
                    return "Недостаточно прав для выполнения этого дествия";
                }
            }
            else
            {
                return "Ошибка. Ответьте этим сообщением на сообщения пользователя ранг, которого надо изменить";
            }
        }
        //When the conditions are met, it increases the rank of users to the one specified in the chat settings
        private static string SetDefaultAdmins(long chatid, long userid)
        {
            //Request a list of conversation administrators from telegram
            using (HttpClientHandler hld = new HttpClientHandler())
            {
                using (HttpClient cln = new HttpClient())
                {
                    using (var resp = cln.GetAsync($"https://api.telegram.org/bot" + botToken + $"/getChatAdministrators?chat_id=" + chatid.ToString()).Result)
                    {
                        var json = resp.Content.ReadAsStringAsync().Result;
                        if (!string.IsNullOrEmpty(json))
                        {
                            //Parse request from JSON
                            ChatAdministrators admins = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatAdministrators>(json);
                            if (admins.result != null)
                            {
                                //Find the creator
                                long creatorId = 0;
                                foreach (var admin in admins.result)
                                {
                                    if (admin.status == "creator")
                                    {
                                        creatorId = admin.user.id;
                                    }
                                }
                                //Verify that the user who initiated the request is the creator
                                if (userid == creatorId)
                                {
                                    foreach (var admin in admins.result)
                                    {
                                        string sql = $"UPDATE `users` SET `Admin` = '{admin.status}' WHERE `users`.`ID` = {chatid.ToString() + admin.user.id.ToString()};";
                                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                                        int rowCount = cmd.ExecuteNonQuery();
                                    }
                                    return "Администраторы успешно обновлены";
                                }
                                else
                                {
                                    return "Команда доступна только создателю чата";
                                }
                            }
                            else
                            {
                                return "Неизвестная ошибка, попробуйте немного позднее";
                            }
                        }
                        else
                        {
                            return "Неизвестная ошибка, попробуйте немного позднее";
                        }
                    }
                }
            }
        }
        //Checks the user's rank, if the rank is not specified, then sets it according to the chat settings
        private static string AdminStatus(long chatid, long userid)
        {
            //Request a list of user ranks from database
            string sql = "SELECT Admin FROM users WHERE ID =" + (chatid.ToString() + userid.ToString());
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            string adminStatus = (string)cmd.ExecuteScalar();
            //If the rank is not specified
            if (adminStatus != "0")
            {
                return adminStatus;
            }
            else
            {
                //request list of chat administrators from telegram api
                using (HttpClientHandler hld = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        using (var resp = cln.GetAsync($"https://api.telegram.org/bot" + botToken + $"/getChatAdministrators?chat_id=" + chatid.ToString()).Result)
                        {
                            var json = resp.Content.ReadAsStringAsync().Result;
                            //Parse response to JSON
                            if (!string.IsNullOrEmpty(json))
                            {
                                ChatAdministrators admins = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatAdministrators>(json);
                                if (admins.result != null)
                                {
                                    //Check user in list of administrators
                                    foreach (var admin in admins.result)
                                    {
                                        if (admin.user.id == userid)
                                        {
                                            //if user is administrator, update database
                                            sql = $"UPDATE `users` SET `Admin` = '{admin.status}' WHERE `users`.`ID` = {chatid.ToString() + userid.ToString()};";
                                            cmd = new MySqlCommand(sql, conn);
                                            int rowCount = cmd.ExecuteNonQuery();
                                            //return administrator rank
                                            return admin.status;

                                        }
                                        else
                                        {
                                            //else update database and set minimal rank
                                            sql = $"UPDATE `users` SET `Admin` = 'normal' WHERE `users`.`ID` = {chatid.ToString() + userid.ToString()};";
                                            cmd = new MySqlCommand(sql, conn);
                                            int rowCount = cmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                                else
                                {
                                    return "normal";
                                }
                            }
                            else
                            {
                                return "normal";
                            }
                        }
                    }
                }
                return "normal";
            }
        }
        //========================================
        //Chat configuration command
        //========================================
        //**************
        //Voice messane commands
        //**************
        private static bool isVoiceMessengeBlocked(Telegram.Bot.Types.Message message)
        {
            string sql = "SELECT voiceMessangeBlock FROM chats WHERE ID =" + message.Chat.Id;
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            bool isVoiseBlock = (bool)cmd.ExecuteScalar();
            return isVoiseBlock;
        }
        //Block or unblock voice messsage in this chat
        private async static void voiceMessangeCommand(Telegram.Bot.Types.Message message)
        {
            //Get the value from the database and change it to the opposite
            string sql = "SELECT voiceMessangeBlock FROM chats WHERE ID =" + message.Chat.Id;
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            bool isVoiseBlock = (bool)cmd.ExecuteScalar();
            if (isVoiseBlock)
            {
                sql = $"Update chats set voiceMessangeBlock	 = 0 where ID = {message.Chat.Id}";
            }
            else
            {
                sql = $"Update chats set voiceMessangeBlock	 = 1 where ID = {message.Chat.Id}";
            }
            cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();
            return;
        }
        //**************
        //Analyzer commands
        //**************
        //Collects statistics and writes them to the database
        private static void Analyzer(MyMessage mymessage, Telegram.Bot.Types.Message dmessage)
        {
            if (mymessage.message != null)
            {
                //Update last activity
                string sql = $"UPDATE `users` SET `LastActivity` = '{DateTime.Now.ToString()}' WHERE `users`.`Id` = {dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString()};";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                int rowCount = cmd.ExecuteNonQuery();
                if (dmessage.Text != null)
                {
                    //Update message count
                    sql = "SELECT MessageCount FROM users WHERE ID =" + dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString();
                    cmd = new MySqlCommand(sql, conn);
                    int messangecount = (int)cmd.ExecuteScalar();
                    sql = $"UPDATE `users` SET `MessageCount` = '{messangecount + 1}' WHERE `users`.`Id` = {dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString()};";
                    cmd = new MySqlCommand(sql, conn);
                    rowCount = cmd.ExecuteNonQuery();
                }
                if (dmessage.Voice != null || dmessage.VideoNote != null)
                {
                    //Update VoiceMessageCoun
                    sql = "SELECT VoiceMessageCount FROM users WHERE ID =" + dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString();
                    cmd = new MySqlCommand(sql, conn);
                    int voicemessangecount = (int)cmd.ExecuteScalar();
                    sql = $"UPDATE `users` SET `VoiceMessageCount` = '{voicemessangecount + 1}' WHERE `users`.`Id` = {dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString()};";
                    cmd = new MySqlCommand(sql, conn);
                    rowCount = cmd.ExecuteNonQuery();
                }
                if (dmessage.Sticker != null)
                {
                    //Update StikerCount
                    sql = "SELECT StikerCount FROM users WHERE ID =" + dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString();
                    cmd = new MySqlCommand(sql, conn);
                    int StikerCount = (int)cmd.ExecuteScalar();
                    sql = $"UPDATE `users` SET `StikerCount` = '{StikerCount + 1}' WHERE `users`.`Id` = {dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString()};";
                    cmd = new MySqlCommand(sql, conn);
                    rowCount = cmd.ExecuteNonQuery();
                }

            }
        }
        //**************
        //Database query commands
        //**************
        private static object GetNickname(long chatid, long userid)
        {
            string sql = "SELECT Nickname FROM users WHERE ID =" + chatid.ToString() + userid.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            string nickName = (string)cmd.ExecuteScalar();
            return nickName;
        }
        private static string GetLastActivity(long chatid, long userid)
        {
            string sql = "SELECT LastActivity FROM users WHERE ID =" + chatid.ToString() + userid.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            string LastActivity = (string)cmd.ExecuteScalar();
            return LastActivity;
        }
        private static long GetStickerCount(long chatid, long userid)
        {
            string sql = "SELECT StikerCount FROM users WHERE ID =" + chatid.ToString() + userid.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int StikerCount = (int)cmd.ExecuteScalar();
            return StikerCount;
        }
        private static long GetVoiceMessageCount(long chatid, long userid)
        {
            string sql = "SELECT VoiceMessageCount FROM users WHERE ID =" + chatid.ToString() + userid.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int VoiceMessageCount = (int)cmd.ExecuteScalar();
            return VoiceMessageCount;
        }
        private static int GetMessageCount(long chatid, long userid)
        {
            string sql = "SELECT MessageCount FROM users WHERE ID =" + chatid.ToString() + userid.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int MessageCount = (int)cmd.ExecuteScalar();
            return MessageCount;
        }
        private static int GetWarnsCount(long chatid, long userid)
        {
            string sql = "SELECT Warns FROM users WHERE ID =" + chatid.ToString() + userid.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int nickName = (int)cmd.ExecuteScalar();
            return nickName;
        }
        //****************************
        //User initialization commands
        //****************************
        private async static void CreateThisUserInDB(long chatid, long userid, string firstName)
        {
            string sql = $"INSERT INTO `users` (`Number`, `ID`, `Admin`, `Chat_id`, `Nickname`, `User_ID`,`IsMute`, `Warns`,`MessageCount`,`VoiceMessageCount`,`StikerCount`, `LastActivity` ) VALUES (NULL, '{chatid.ToString() + userid.ToString()}', '0', '{chatid}', '{firstName}', '{userid}', '0', '0', '0', '0', '0', '{DateTime.Now.ToString()}')";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();
        }
        private static bool isThisUserInDB(long chatid, long userid)
        {
            string sql = $"SELECT COUNT(id) FROM `users` WHERE id=" + chatid.ToString() + userid.ToString();
            Console.WriteLine(sql);
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            bool isThisUserInDB = false;
            long req = (Int64)cmd.ExecuteScalar();
            if (req == 1)
            {
                isThisUserInDB = true;
            }
            return isThisUserInDB;
        }
        //****************************
        //Chat initialization commands
        //****************************
        private static void CreateThisChatInDb(Telegram.Bot.Types.Message message)
        {
            string sql = $"INSERT INTO `chats` (`Number`, `ID`, `IsVIP`, `voiceMessangeBlock`) VALUES (NULL, {message.Chat.Id}, '0', '0');";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();
        }
        private static bool isThisChatInDB(Telegram.Bot.Types.Message message)
        {
            string sql = "SELECT Count(id) FROM chats WHERE id = " + message.Chat.Id;
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            bool isThisChatInDB = false;
            long req = (Int64)cmd.ExecuteScalar();
            if (req == 1)
            {
                isThisChatInDB = true;
            }
            return isThisChatInDB;

        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            try
            {
                conn.Open(); 
            }
            catch
            {
                Console.WriteLine("Ошибка соединения с базой данных");
            }
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}