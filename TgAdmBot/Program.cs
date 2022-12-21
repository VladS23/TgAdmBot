using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using TgAdmBot.Database;
using System.Reflection.Metadata.Ecma335;

namespace TgAdmBot
{
    class Program
    {
        //This list help to determine admin hierarсhy
        private static string[] AdmRangs = { "creator", "administrator", "moderator", "helper", "normal" };
        //Initializing botapi and database connection
        private static string botToken = "5328960378:AAFJx-f2ZnMJY_545JW7GGiJTvCuJDwuNMY";
        private static ITelegramBotClient bot = new TelegramBotClient(botToken);
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //Servise output
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                //I use two object to work with the messange,
                //since each of them does not implement all the functionality of the second one
                string strupdate = Newtonsoft.Json.JsonConvert.SerializeObject(update);
                MyMessage mymessage = Newtonsoft.Json.JsonConvert.DeserializeObject<MyMessage>(strupdate);
                Telegram.Bot.Types.Message message = update.Message;
                //Initializing the chat
                if (message.Text != null)
                {
                    if (!isThisChatInDB(message))
                    {
                        CreateThisChatInDb(message);
                        await botClient.SendTextMessageAsync(message.Chat, "Привет! Я Амалия, для корректной работы я должна быть администратором, чтобы узнать мои возможности /help");

                    }
                    //Initializing the user
                    if (!isThisUserInDB(mymessage.message.chat.id, mymessage.message.from.id))
                    {
                        CreateThisUserInDB(mymessage.message.chat.id, mymessage.message.from.id, mymessage.message.from.first_name);
                    }
                    Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == mymessage.message.chat.id);
                    Database.User user = chat.Users.Single(user => user.TelegramUserId == mymessage.message.from.id);
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
                        if (message.Text.Split("@").Length == 2 && message.Text.Split("@").Length == 1)
                        {
                            message.Text = message.Text.Replace($"@{message.Text.Split("@")[message.Text.Split("@").Length - 1]}", "");
                        }
                        //Processing help commands
                        if (message.Text.ToLower() == "/help")
                        {
                            user.LastMessage = message.Text;
                            BotDatabase.db.SaveChanges();
                            await botClient.SendTextMessageAsync(message.Chat, "Выберите какой раздел команд вас интересует \n 1. Развлечения \n 2. Настройка беседы \n 3. Администрирование");
                            return;
                        }
                        if (message.Text.ToLower() == "1" && user.LastMessage == "/help")
                        {
                            user.LastMessage = message.Text;
                            BotDatabase.db.SaveChanges();
                            await botClient.SendTextMessageAsync(message.Chat, "Список развлекательных команд\n1. Ник + имя - установит вам в качестве ника \"имя\"\n2. Участники - выведет список всех участников беседы \n3. Стата - выведет вашу статистику, пользователи с рангом модератор и выше могут посмотреть статистику пользователей с рангом меньше, чем у них, если напишут это сообщение в ответ на сообщение пользователя, статистику которого необходимо просмотреть\n4. Рнд число1-число2 - сгенерирует случайное число из указанного промежутка\n5. Вбр вариант1 или вариант2 - выберет один из указанных вариантов\n6. Me действие - выведет сообщение вида: \"пользователь1 действие пользователь2\" (пользователь2 указывается путем ответа на его сообщение)\n 7. кт + действие - выведет: *Случайный участник беседы* действие\n 8. Вртн + событие - предположит вероятность какого-то события");
                            return;
                        }
                        if (message.Text.ToLower() == "2" && user.LastMessage == "/help")
                        {
                            user.LastMessage = message.Text;
                            BotDatabase.db.SaveChanges();
                            await botClient.SendTextMessageAsync(message.Chat, "Список доступных настроек беседы\n1. setdefaultadmins - назначит администраторов беседы в соответсвтвие с тем, как расставлены права в телеграм, может использоваться только создателем беседы\n 2. /voicemessange заблокирует или разблокирует голосовые и видеосообщения, по умолчанию разблокировано, может применяться администраторами или создателем\n3. /setwarninglimitaction - установка наказания за превышение количества предупреждений, по умолчанию mute");
                            return;
                        }
                        if (message.Text.ToLower() == "3" && user.LastMessage == "/help")
                        {
                            user.LastMessage = message.Text;
                            BotDatabase.db.SaveChanges();
                            await botClient.SendTextMessageAsync(message.Chat, "Список доступных административных действий:\n 1. Назначения ранга пользователям. Ранг создателя сразу выдается создателем беседы, остальные ранги могут быть назначены пользователем с рангом выше. Чтобы назначить ранг необходимо написать одну из следующих команд в ответ на сообщение пользователя, которому необходимо назначить ранг\n   /admin\n    /moder\n    /helper\n    /normal\n 2. /mute Если ваш ранг выше или равен модератору и выше, чем у пользователя, на сообщение, которого вы ответили, то вы запретите или разрешите ему писать сообщения, не работает на пользователей, которым в настройках беседы телеграм установлен ранг администратор\n 3. /muted Выведет список всех, кому запрещено писать сообщения\n 4. /ban Исключит пользователя из беседы и добавит его в черный список чата, данная команда доступна только создателю и администратора, не работает на пользователей, котрым в настройках беседы в телеграм установлен ранг администратора\n 5. /unban  Исключит пользователя из черного списка чата, команда доступна администраторам и создателям\n 6. /warn Выдаст пользователю предупреждение, по достижению трех предупреждений он будет исключен из беседы, команда доступна создателю или администраторам\n 7. /warns выведет список предупрежденных пользователей беседы, доступна создателю или администраторам \n8. /unwarn снимет с пользователя все предупреждения");
                            return;
                        }
                        //This command should be used to search for the chat creator
                        //if it is defined incorrectly during initialization
                        if (message.Text.ToLower() == "setdefaultadmins")
                        {
                            user.LastMessage = message.Text;
                            BotDatabase.db.SaveChanges();
                            await botClient.SendTextMessageAsync(message.Chat, SetDefaultAdmins(message.Chat.Id, message.From.Id));
                            return;
                        }
                        //Entertainment commands
                        if (message.Text.ToLower() == "/rules")
                        {
                            user.LastMessage = message.Text;
                            BotDatabase.db.SaveChanges();
                            await botClient.SendTextMessageAsync(message.Chat, GetRules(message.Chat.Id));
                            return;
                        }
                        if (message.Text.ToLower().Contains("/setrules"))
                        {
                            user.LastMessage = message.Text;
                            BotDatabase.db.SaveChanges();
                            await botClient.SendTextMessageAsync(message.Chat, SetRules(message.Chat.Id, message.From.Id, message.Text));
                            return;
                        }
                        if (message.Text.ToLower() == "актив")
                        {
                            user.LastMessage = message.Text;
                            BotDatabase.db.SaveChanges();
                            await botClient.SendTextMessageAsync(message.Chat, GetUsersActivity(message.Chat.Id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            return;
                        }
                        if (message.Text.Length > 4)
                        {
                            if (message.Text.ToLower()[0] == 'н' && message.Text.ToLower()[1] == 'и' && message.Text.ToLower()[2] == 'к' && message.Text.ToLower()[3] == ' ')
                            {
                                user.LastMessage = message.Text;
                                BotDatabase.db.SaveChanges();
                                await botClient.SendTextMessageAsync(message.Chat, SetNickname(mymessage.message.chat.id, mymessage.message.from.id, mymessage.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;
                            }
                        }
                        if (message.Text.Length == 9)
                        {
                            if (message.Text.ToLower() == "участники")
                            {
                                user.LastMessage = message.Text;
                                BotDatabase.db.SaveChanges();
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
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
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
                                        user.LastMessage = message.Text;
                                        BotDatabase.db.SaveChanges();
                                        await botClient.SendTextMessageAsync(message.Chat.Id, GetStatistics(mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                    else
                                    {
                                        user.LastMessage = message.Text;
                                        BotDatabase.db.SaveChanges();
                                        await botClient.SendTextMessageAsync(message.Chat.Id, "Недостаточно прав на выполнение этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                }

                            }

                        }
                        if (message.Text.ToLower() == "стата чата")
                        {
                            user.LastMessage = message.Text;
                            BotDatabase.db.SaveChanges();
                            await botClient.SendTextMessageAsync(message.Chat.Id, ChatInfo(message.Chat.Id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            return;
                        }
                        if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 4)
                        {
                            if (message.Text.Length > 6)
                            {
                                if (message.Text.ToLower()[0] == 'р' && message.Text.ToLower()[1] == 'н' && message.Text.ToLower()[2] == 'д' && message.Text.ToLower()[3] == ' ')
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
                                    await botClient.SendTextMessageAsync(message.Chat, GetRandomNumber(mymessage.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.Length > 10)
                            {
                                if (message.Text.ToLower()[0] == 'в' && message.Text.ToLower()[1] == 'б' && message.Text.ToLower()[2] == 'р' && message.Text.ToLower()[3] == ' ')
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
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
                                        user.LastMessage = message.Text;
                                        BotDatabase.db.SaveChanges();
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
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
                                    await botClient.SendTextMessageAsync(message.Chat, Who(mymessage.message.text, message.Chat.Id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.Length > 5)
                            {
                                if (message.Text.ToLower()[0] == 'в' && message.Text.ToLower()[1] == 'р' && message.Text.ToLower()[2] == 'т' && message.Text.ToLower()[3] == 'н' && message.Text.ToLower()[4] == ' ')
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
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
                                user.LastMessage = message.Text;
                                BotDatabase.db.SaveChanges();
                                await botClient.SendTextMessageAsync(message.Chat, SetAdminStatus(UserRights.administrator, mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;
                            }
                            if (message.Text.ToLower().Trim() == "/moder")
                            {
                                user.LastMessage = message.Text;
                                BotDatabase.db.SaveChanges();
                                await botClient.SendTextMessageAsync(message.Chat, SetAdminStatus(UserRights.moderator, mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;

                            }
                            if (message.Text.ToLower().Trim() == "/helper")
                            {
                                user.LastMessage = message.Text;
                                BotDatabase.db.SaveChanges();
                                await botClient.SendTextMessageAsync(message.Chat, SetAdminStatus(UserRights.helper, mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;

                            }
                            if (message.Text.ToLower().Trim() == "/normal")
                            {
                                user.LastMessage = message.Text;
                                BotDatabase.db.SaveChanges();
                                await botClient.SendTextMessageAsync(message.Chat, SetAdminStatus(UserRights.normal, mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
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
                                            user.LastMessage = message.Text;
                                            BotDatabase.db.SaveChanges();
                                            Unmute(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                            await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) разрешил пользователю [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id}) писать сообщения", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                            return;

                                        }
                                        else
                                        {
                                            user.LastMessage = message.Text;
                                            BotDatabase.db.SaveChanges();
                                            Mute(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                            await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) запретил пользователю [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id}) писать сообщения, чтобы вновь разрешить данному пользователю писать введите " + "\"/mute\" в ответ на его сообщения", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        user.LastMessage = message.Text;
                                        BotDatabase.db.SaveChanges();
                                        await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                }
                                else
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
                                    await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщением на сообщение пользователя, которому необходимо запретить писать", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (mymessage.message.text.ToLower().Trim() == "/muted")
                            {
                                if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 2)
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
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
                                        user.LastMessage = message.Text;
                                        BotDatabase.db.SaveChanges();
                                        Ban(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                        await botClient.SendTextMessageAsync(message.Chat, $"[{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) забанил [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id}) чтобы вернуть данного пользователя обратно администратор или создатель должен написать /unban в ответ на любое сообщение пользователя, а затем пригласить его в чат или вручную удалить его из черного списка чата, а затем пригласить ", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                    else
                                    {
                                        user.LastMessage = message.Text;
                                        BotDatabase.db.SaveChanges();
                                        await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                }
                                else
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
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
                                        user.LastMessage = message.Text;
                                        BotDatabase.db.SaveChanges();
                                        UnBan(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                        await botClient.SendTextMessageAsync(message.Chat, $"[{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) разбанил [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id}) теперь он вновь может быть приглашен в чат", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                    else
                                    {
                                        user.LastMessage = message.Text;
                                        BotDatabase.db.SaveChanges();
                                        await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав на выполнения этого действия", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        return;
                                    }
                                }
                                else
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
                                    await botClient.SendTextMessageAsync(message.Chat, "Ответьте этим сообщениес на сообщение пользователя, которому необходимо запретить писать", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.ToLower().Contains("/warn"))
                            {
                                if (mymessage.message.reply_to_message != null)
                                {
                                    if (Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 1 && Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) < Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)))
                                    {
                                        if (user.WarnsCount < chat.WarnsLimit)
                                        {
                                            user.LastMessage = message.Text;
                                            BotDatabase.db.SaveChanges();
                                            Warn(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id);
                                            if (message.Text.Length > 6)
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) выдал предупреждение пользователю [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id})\nПо причине:{message.Text.Substring(5)}\nПредупреждений до исключения {chat.WarnsLimit - user.WarnsCount}", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                                return;
                                            }
                                            else
                                            {
                                                await botClient.SendTextMessageAsync(message.Chat, $"Пользователь [{GetNickname(mymessage.message.chat.id, mymessage.message.from.id)}](tg://user?id={mymessage.message.from.id}) выдал предупреждение пользователю [{GetNickname(mymessage.message.reply_to_message.chat.id, mymessage.message.reply_to_message.from.id)}](tg://user?id={mymessage.message.reply_to_message.from.id})\nПредупреждений до исключения {chat.WarnsLimit - user.WarnsCount}", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                                return;
                                            }
                                        }
                                        else
                                        {
                                            await botClient.SendTextMessageAsync(message.Chat, WarningLimitAction(message.Chat.Id, message.ReplyToMessage.From.Id));
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
                                        user.LastMessage = message.Text;
                                        BotDatabase.db.SaveChanges();
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
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
                                    await botClient.SendTextMessageAsync(message.Chat, GetWarnedUsers(mymessage.message.chat.id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.ToLower().Trim() == "/voicemessange")
                            {
                                if (Array.IndexOf(AdmRangs, AdminStatus(message.Chat.Id, message.From.Id)) <= 1)
                                {
                                    voiceMessangeCommand(message);
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
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
                            if (message.Text.ToLower().Contains("/setwarninglimitaction"))
                            {
                                user.LastMessage = message.Text;
                                BotDatabase.db.SaveChanges();
                                await botClient.SendTextMessageAsync(message.Chat, SetWarningLimitAction(message.Chat.Id, message.From.Id, message.Text));
                                return;
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
        }
        private static string StrToYesNo(string s)
        {
            if (s == "1")
            {
                return "Да";
            }
            return "Нет";
        }

        private static string ChatInfo(long chatid)
        {
            long messangeCount = 0;
            long activChatUser = 0;
            string info = "";
            Database.Chat chat = BotDatabase.db.Chats.First(chat => chat.TelegramChatId == chatid);

            return (
               "📊 Информация о чате:\n"
            + $"📈 ID чата: {chat.TelegramChatId}\n"
            + $"💎 VIP чат: {chat.Status.ToString()}\n"
            + $"🎧 Голосовые сообщения запрещены: {(chat.VoiceMessagesDisallowed ? "Да" : "Нет")}\n"
            + $"⚖️ Наказание за превышение лимита предупреждений: {(chat.WarnsLimitAction == WarnsLimitAction.mute ? "Мут" : "Ничего")}\n"
            + $"👨‍💻 Активные пользователи: {chat.Users.Count}\n"
            + $"✉️ Сообщений всего: {chat.MessagesCount}\n"
                );
        }

        private static string SetWarningLimitAction(long chatid, long userid, string text)
        {
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            if (chat.Users.Single(user => user.TelegramUserId == userid).UserRights == UserRights.administrator)
            {
                string[] command = text.Split(' ').Length > 1 ? text.Split(' ') : new string[] { "", "" };

                switch (command[1])
                {
                    case "mute":
                        chat.WarnsLimitAction = WarnsLimitAction.mute;
                        BotDatabase.db.SaveChanges();
                        return "Теперь после достижения лимита предупреждений пользователю будет запрещено писать";
                    case "ban":
                        chat.WarnsLimitAction = WarnsLimitAction.ban;
                        BotDatabase.db.SaveChanges();
                        return "Теперь после достижения лимита предупреждений пользователь будет удален";
                    default:
                        return "Неизвестный аргумент. Ожидалось mute или ban";
                }
            }
            else
            {
                return "Не достаточно прав!";
            }

        }

        private static void CheckMuted()
        {
            foreach (Database.Chat chat in BotDatabase.db.Chats)
            {
                foreach (Database.User user in chat.Users.Where(user => user.IsMuted && user.UnmuteTime < DateTime.Now).ToList())
                {
                    Unmute(chat.TelegramChatId, user.TelegramUserId);
                }
            }
        }

        private static string WarningLimitAction(long chatid, long userid)
        {
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            Database.User user = chat.Users.Single(user => user.TelegramUserId == userid);

            switch (chat.WarnsLimitAction)
            {
                case WarnsLimitAction.ban:
                    Ban(chatid, userid);
                    return "Пользователь был удален в связи с превышением лимита предупреждений";
                case WarnsLimitAction.mute:
                    Mute(chatid, userid);
                    return "Пользователю запрещено писать сообщения в связи с превышением лимита предупреждений";
                default:
                    return "Неизвестная ошибка";
            }
        }

        //======================
        //entertainment messages
        //======================
        private static string GetRules(long chatid)
        {
            return BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid).Rules;
        }
        private static string SetRules(long chatid, long userid, string messagetext)
        {
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);

            //Checking the validity of the nickname
            if (chat.Users.Single(user => user.TelegramUserId == userid).UserRights == UserRights.administrator)
            {
                if (messagetext.Length > 5)
                {
                    if (messagetext.Length < 10000)
                    {
                        try
                        {
                            if (!messagetext.ToLower().Contains("drop"))
                            {
                                //Process the string and write it to the database
                                string rules = messagetext.Substring(9);
                                chat.Rules = rules;
                                return $"Правила чата установлены";
                            }
                            else
                            {
                                return "Использованы недопустимые символы";
                            }
                        }
                        catch
                        {
                            return "Использованы недопустимые символы";
                        }
                    }
                    else
                    {
                        return "Правила слишком длинные";
                    }
                }
                else
                {
                    return "Правила слишком короткие";
                }
            }
            else
            {
                return "Только админы и владелец могут изменять правила";
            }
        }
        //**************
        //Nicknames Commands
        //**************
        //Сreates a message about all user nicknames
        private static string GetChatNicknames(long chatid)
        {
            string result = "Участники беседы:\n";
            int index = 1;
            foreach (Database.User user in BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid).Users)
            {
                result = $"{result}{index}. {user.Nickname}\n";
                index += 1;
            }
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
                        string nickname = messagetext.Substring(4);
                        Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
                        chat.Users.Single(user => user.TelegramUserId == userid).Nickname = nickname;
                        BotDatabase.db.SaveChanges();
                        return $"Вам установлен ник \"[{nickname}](tg://user?id={userid})\", теперь бот будет использовать его при взаимодействии с вами";
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
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            Database.User user = chat.Users[new Random().Next(0, chat.Users.Count - 1)];
            return $"[{user.Nickname}](tg://user?id={user.TelegramUserId}) {text}";
        }
        //Build message about users activity
        private static string GetUsersActivity(long chatid)
        {
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            string result = "Активность пользователей:\n";
            foreach (Database.User user in chat.Users)
            {
                result = $"{result} {user.Nickname} - {user.LastActivity.ToLongDateString}\n";
            }
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
                Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
                Database.User user = chat.Users.Single(user => user.TelegramUserId == userid);

                info = "📈 Информация о " + username + "\n";
                try
                {
                    info = info + "👤 Имя: " + tgusername + "\n";
                }
                catch
                {

                }
                info = info + $"👥 Ник : [{GetNickname(chatid, userid)}](tg://user?id={userid})" + "\n";
                info = info + "👑 Ранг: " + AdminStatus(chatid, userid) + "\n";
                if (IsThisUserMute(chatid, userid))
                {
                    info = info + "🚫 Запрещено писать сообщения\n";
                }
                else
                {
                    info = info + "👨‍💻 Разрешено писать сообщения\n";
                }
                info = info + "🕒 Время последней активности: " + GetLastActivity(chatid, userid) + "\n";
                info = info + $"⛔️ Количество предупреждений {user.WarnsCount}/{chat.WarnsLimit}\n";
                info = info + $"✉️ Отправлено сообщений: {user.MessagesCount}\n";
                info = info + $"🎤 Отправлено голосовых сообщений: {user.VoiceMessagesCount}\n";
                info = info + $"😄 Отправлено стикеров: {user.StickerMessagesCount}\n";
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
                Random rnd = new Random();
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
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            List<Database.User> users = chat.Users.Where(user => user.WarnsCount > 0).ToList();
            //If warned users are exist than retuln result string else return else string
            if (users.Count > 0)
            {
                string result = "Предупреждённые пользователи:\n";
                int index = 1;
                foreach (Database.User user in users)
                {
                    result = $"{result}{index}. {user.Nickname}({user.TelegramUserId}) {user.WarnsCount}/{chat.WarnsLimit}\n";
                    index += 1;
                }
                return result;
            }
            else
            {
                return "Нет предупрежденных пользователей";
            }
        }
        //Removes all warnings
        private static void Unwarn(long chatid, long userid)
        {
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            chat.Users.Single(user => user.TelegramUserId == userid).WarnsCount -= 1;
            BotDatabase.db.SaveChanges();
        }
        //Adds another warning
        private static void Warn(long chatid, long userid)
        {
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            chat.Users.Single(user => user.TelegramUserId == userid).WarnsCount += 1;
            BotDatabase.db.SaveChanges();
        }
        //Builds a message about muted user
        private static string GetMutedUsers(long chatid)
        {
            Database.Chat chat = BotDatabase.db.Chats.First(chat => chat.TelegramChatId == chatid);
            List<Database.User> mutedUsers = chat.Users.Where(user => user.IsMuted == true).ToList();
            string mutedUsersText = "";
            if (mutedUsers.Count < 1)
            {
                mutedUsersText = "Нет замьюченых пользователей";
            }
            else
            {
                for (int index = 0; index < mutedUsers.Count; index++)
                {
                    mutedUsersText = $"{mutedUsersText}{index}.{mutedUsers[index].Nickname}\n";
                }
            }

            return mutedUsersText;
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
                        //and update database cell                        Console.WriteLine("vhjbn");
                        Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
                        chat.Users.Single(user => user.TelegramUserId == userid).IsMuted = true;
                        BotDatabase.db.SaveChanges();
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
                        string restext = $"https://api.telegram.org/bot{botToken}/restrictChatMember?user_id={userid}&chat_id={chatid}&until_date={((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() + 35}";
                        using (var request = cln.GetAsync(restext).Result)
                        {

                        }
                        //and update database cell
                        Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
                        chat.Users.Single(user => user.TelegramUserId == userid).IsMuted = false;
                        BotDatabase.db.SaveChanges();
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
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            return chat.Users.Single(user => user.TelegramUserId == userid).IsMuted;
        }
        //**************
        //Admin Assignment Commands
        //**************
        //If all conditions are met, sets the user's rank
        private static string SetAdminStatus(UserRights rights, MyMessage mymessage)
        {
            //If the request came from a user with sufficient rank sets the specified rank to the specified user
            if (mymessage.message.reply_to_message != null)
            {
                Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == mymessage.message.chat.id);
                //Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) < Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)))
                if (Array.IndexOf(AdmRangs, chat.Users.Single(user => user.TelegramUserId == mymessage.message.from.id).UserRights.ToString())> Array.IndexOf(AdmRangs, rights.ToString()))
                {
                    chat.Users.Single(user => user.TelegramUserId == mymessage.message.reply_to_message.from.id).UserRights = rights;
                    BotDatabase.db.SaveChanges();
                    return "Выполнено";
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
                                    Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
                                    foreach (Result admin in admins.result)
                                    {
                                        try
                                        {
                                            chat.Users.Single(user => user.TelegramUserId == admin.user.id).UserRights = UserRights.administrator;
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    chat.Users.Single(user => user.TelegramUserId == creatorId).UserRights = UserRights.creator;
                                    BotDatabase.db.SaveChanges();
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

            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            Console.WriteLine(chat.Users.Single(user => user.TelegramUserId == userid).TelegramUserId);
            Console.WriteLine(chat.Users.Single(user => user.TelegramUserId == userid).UserRights);
            return chat.Users.Single(user => user.TelegramUserId == userid).UserRights.ToString();
        }
        //========================================
        //Chat configuration command
        //========================================
        //**************
        //Voice messane commands
        //**************
        private static bool isVoiceMessengeBlocked(Telegram.Bot.Types.Message message)
        {

            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == message.Chat.Id);
            return chat.VoiceMessagesDisallowed;
        }
        //Block or unblock voice messsage in this chat
        private async static void voiceMessangeCommand(Telegram.Bot.Types.Message message)
        {
            //Get the value from the database and change it to the opposite
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == message.Chat.Id);
            chat.VoiceMessagesDisallowed = !chat.VoiceMessagesDisallowed;
            BotDatabase.db.SaveChanges();
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
                Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == mymessage.message.chat.id);
                Database.User user = chat.Users.Single(user => user.TelegramUserId == mymessage.message.from.id);
                //Update last activity
                user.LastActivity = DateTime.Now;
                if (dmessage.Text != null)
                {
                    //Update message count
                    user.MessagesCount += 1;
                }
                if (dmessage.Voice != null || dmessage.VideoNote != null)
                {
                    user.VoiceMessagesCount += 1;
                }
                if (dmessage.Sticker != null)
                {
                    //Update StikerCount
                    user.StickerMessagesCount += 1;
                }
                BotDatabase.db.SaveChanges();
            }
        }
        //**************
        //Database query commands
        //**************
        private static object GetNickname(long chatid, long userid)
        {

            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            Database.User user = chat.Users.Single(user => user.TelegramUserId == userid);
            return user.Nickname;
        }
        private static string GetLastActivity(long chatid, long userid)
        {
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            Database.User user = chat.Users.Single(user => user.TelegramUserId == userid);
            return user.LastActivity.ToLongTimeString();
        }
        //****************************
        //User initialization commands
        //****************************
        private async static void CreateThisUserInDB(long chatid, long userid, string firstName)
        {
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            chat.Users.Add(new Database.User { Nickname = firstName, TelegramUserId = userid });
        }
        private static bool isThisUserInDB(long chatid, long userid)
        {
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
            Console.WriteLine(chat.Users.Count);
            Console.WriteLine(chat.Users.Where(user => user.TelegramUserId == userid).ToList().Count > 0);
            return chat.Users.Where(user => user.TelegramUserId == userid).ToList().Count > 0;
        }
        //****************************
        //Chat initialization commands
        //****************************
        private static void CreateThisChatInDb(Telegram.Bot.Types.Message message)
        {
            BotDatabase.db.Chats.Add(new Database.Chat { TelegramChatId = message.Chat.Id });
            BotDatabase.db.SaveChanges();
            SetDefaultAdmins(message.Chat.Id, message.From.Id);
        }
        private static bool isThisChatInDB(Telegram.Bot.Types.Message message)
        {
            //Console.WriteLine("Chat" + (BotDatabase.db.Chats.Where(chat => chat.TelegramChatId == message.Chat.Id).ToList().Count > 0));
            return BotDatabase.db.Chats.Where(chat => chat.TelegramChatId == message.Chat.Id).ToList().Count > 0;
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        static void Main(string[] args)
        {
            BotDatabase db = new BotDatabase();
            Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);
            //Thread MytedChecker = new Thread(CheckMuted);
            //MytedChecker.Start();
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