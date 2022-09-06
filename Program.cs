using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using MySql.Data.MySqlClient;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace TgAdmBot
{
    class Program
    {
        private static string[] AdmRangs = { "creator", "administrator", "moderator", "helper", "normal" };
        private static string botToken = "5328960378:AAH1fskxZcH3GEXMVKHROuhfnNJQsSj8gvU";
        private static ITelegramBotClient bot = new TelegramBotClient(botToken);
        private static MySqlConnection conn = new MySqlConnection("server=localhost; port=3306; username=root; password=root; database=tgadmbot");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                string strupdate = Newtonsoft.Json.JsonConvert.SerializeObject(update);
                MyMessage mymessage = Newtonsoft.Json.JsonConvert.DeserializeObject<MyMessage>(strupdate);
                Telegram.Bot.Types.Message message = update.Message;
                //Console.WriteLine(message.From.);
                //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(message.Chat));
                if (!isThisChatInDB(message))
                {
                    CreateThisChatInDb(message);
                }
               // Console.WriteLine(isThisUserInDB(message));
                if (!isThisUserInDB(mymessage.message.chat.id, mymessage.message.from.id))
                {
                    CreateThisUserInDB(mymessage.message.chat.id, mymessage.message.from.id, mymessage.message.from.first_name);
                }
                Analyzer(mymessage, message);
                if (mymessage.message.reply_to_message != null)
                {
                    if (!isThisUserInDB(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id))
                    {
                        CreateThisUserInDB(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id, mymessage.message.reply_to_message.from.first_name);
                    }
                }
                if (message.Text != null)
                {
                    if (message.Text.ToLower() == "setdefaultadmins")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, SetDefaultAdmins(message.Chat.Id, message.From.Id));
                        return;
                    }
                    if (message.Text.Length > 4)
                    {
                        if (message.Text.ToLower()[0] == 'н' && message.Text.ToLower()[1] == 'и' && message.Text.ToLower()[2] == 'к' && message.Text.ToLower()[3] == ' ')
                        {
                            await botClient.SendTextMessageAsync(message.Chat, SetNickName(mymessage.message.chat.id, mymessage.message.from.id, mymessage.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            return;
                        }
                    }
                    if (message.Text.Length == 4)
                    {
                        if (message.Text.ToLower()=="ники")
                        {
                            await botClient.SendTextMessageAsync(message.Chat, GetChatNicknames(mymessage.message.chat.id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                            return;
                        }
                    }
                    if (message.Text.Length == 5)
                    {
                        if (message.Text.ToLower()=="стата")
                        {
                            if (mymessage.message.reply_to_message == null)
                            {
                                await botClient.SendTextMessageAsync(message.Chat.Id, GetStatistics(mymessage), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;
                            }
                            else
                            {
                                if(Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) <= 2 && Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.from.id)) < Array.IndexOf(AdmRangs, AdminStatus(mymessage.message.chat.id, mymessage.message.reply_to_message.from.id)))
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
                                await botClient.SendTextMessageAsync(message.Chat, GetRandomNumber(mymessage.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
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
                    //Console.WriteLine(message.From.Id);
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
                if ((message.Voice != null || message.VideoNote != null) & isVoiceMessengeBlocked(message))
                {
                    await botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                    return;
                }  
            }
        }

        private static string GetStatistics(MyMessage mymessage)
        {
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
                info = info + "⛔️ Количество предупреждений " + GetWarnsCount(chatid, userid) + "/3\n";
                info = info + "✉️ Отправлено сообщений: " + GetMessageCount(chatid, userid) + "\n";
                info = info + "🎤 Отправлено голосовых сообщений: " + GetVoiceMessageCount(chatid, userid) + "\n";
                info = info + "😄 Отправлено отправленых стикеров: " + GetStickerCount(chatid, userid) + "\n";
            }
            catch
            {
                info = "Возникла неожиданная ошибка";
            }
            return info;
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

        private static void Analyzer(MyMessage mymessage, Telegram.Bot.Types.Message dmessage)
        {
            if (mymessage.message != null)
            {
                if (dmessage.Text != null)
                {
                    string sql = "SELECT MessageCount FROM users WHERE ID =" + dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int messangecount = (int)cmd.ExecuteScalar();
                    sql = $"UPDATE `users` SET `MessageCount` = '{messangecount + 1}' WHERE `users`.`Id` = {dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString()};";
                    cmd = new MySqlCommand(sql, conn);
                    int rowCount = cmd.ExecuteNonQuery();
                }
                if (dmessage.Voice!= null || dmessage.VideoNote != null)
                {
                    string sql = "SELECT VoiceMessageCount FROM users WHERE ID =" + dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int voicemessangecount = (int)cmd.ExecuteScalar();
                    sql = $"UPDATE `users` SET `VoiceMessageCount` = '{voicemessangecount + 1}' WHERE `users`.`Id` = {dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString()};";
                    cmd = new MySqlCommand(sql, conn);
                    int rowCount = cmd.ExecuteNonQuery();
                }
                if (dmessage.Sticker != null)
                {
                    string sql = "SELECT StikerCount FROM users WHERE ID =" + dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString();
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    int StikerCount = (int)cmd.ExecuteScalar();
                    sql = $"UPDATE `users` SET `StikerCount` = '{StikerCount + 1}' WHERE `users`.`Id` = {dmessage.Chat.Id.ToString() + dmessage.From.Id.ToString()};";
                    cmd = new MySqlCommand(sql, conn);
                    int rowCount = cmd.ExecuteNonQuery();
                }
            }
        }

        private static string Probability(string messagetext)
        {
            string mes = messagetext.Substring(4);
            Random rnd = new Random();
            return "Вероятность" + mes + $" {rnd.Next(0, 101)}%";
        }

        private static string Chose(string messagetext)
        {
            try
            {
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

        private static string GetRandomNumber(string messagetext)
        {
            try
            {
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

        private static string GetWarnedUsers(long chatid)
        {
            string sql = $"SELECT `Nickname`, `User_ID`,`Warns`  FROM `users` WHERE `Chat_id`={chatid} AND `Warns`!=0";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            string result = "Предупрежденные пользователи: \n";
            int cnt = 1;
            while (reader.Read())
            {
                result = result + $"{cnt}. [{reader[0].ToString()}](tg://user?id={reader[1].ToString()}) " +reader[2].ToString() +"/3"+ "\n";
                cnt = cnt + 1;
            }
            reader.Close();
            if (cnt > 1)
            {
                return result;
            }
            else
            {
                return "нет предупрежденных пользователей";
            }
        }

        private static void Unwarn(long chatid, long userid)
        {
            string sql = $"UPDATE `users` SET `Warns` = '{0}' WHERE `users`.`Id` = {chatid.ToString() + userid.ToString()};";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();
        }

        private static int GetWarnsCount(long chatid, long userid)
        {
            string sql = "SELECT Warns FROM users WHERE ID =" + chatid.ToString() + userid.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int nickName = (int)cmd.ExecuteScalar();
            return nickName;
        }

        private static void Warn(long chatid, long userid)
        {
            string sql = $"UPDATE `users` SET `Warns` = '{GetWarnsCount(chatid, userid)+1}' WHERE `users`.`Id` = {chatid.ToString() + userid.ToString()};";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();
        }

        private static string GetMutedUsers(long chatid)
        {
            string sql = $"SELECT `Nickname`, `User_ID` FROM `users` WHERE `Chat_id`={chatid} AND `IsMute`=1";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            string result = "Пользователи, которым запрещено писать: \n";
            int cnt = 1;
            while (reader.Read())
            {
                result = result + $"{cnt}. [{reader[0].ToString()}](tg://user?id={reader[1].ToString()})" + "\n";
                cnt = cnt + 1;
            }
            reader.Close();
            if (cnt >  1)
            {
                return result;
            }
            else
            {
                return "все пользователи чата могут писать";
            }
        }

        private static void UnBan(long chatid, long userid)
        {
            try
            {
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

        private static void Ban(long chatid, long userid)
        {
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

        private static void Mute(long chatid, long userid)
        {
            try
            {
                using (HttpClientHandler hndl = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        string restext = $"https://api.telegram.org/bot{botToken}/restrictChatMember?user_id={userid}&chat_id={chatid}";
                        using (var request = cln.GetAsync(restext).Result)
                        {

                        }
                    }
                }
            }
            catch
            {

            }
            string sql = $"Update users set IsMute = 1 where ID = {chatid.ToString()+userid.ToString()}";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();

        }

        private static void Unmute(long chatid, long userid)
        {
            try
            {
                using (HttpClientHandler hndl = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        string restext = $"https://api.telegram.org/bot{botToken}/restrictChatMember?user_id={userid}&chat_id={chatid}&until_date={((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds()+35}";
                        using (var request = cln.GetAsync(restext).Result)
                        {

                        }
                    }
                }
            }
            catch
            {

            }
            string sql = $"Update users set IsMute = 0 where ID = {chatid.ToString() + userid.ToString()}";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();
        }

        private static bool IsThisUserMute(long chatid, long userid)
        {
            string sql = "SELECT IsMute FROM users WHERE ID =" + chatid.ToString() + userid.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            bool nickName = (bool)cmd.ExecuteScalar();
            return nickName;
        }

        private static string GetChatNicknames(long chatid)
        {
            string sql = $"SELECT `Nickname`, `User_ID` FROM `users` WHERE `Chat_id`={chatid}";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            string result = "Ники беседы: \n";
            int cnt = 1;
            while (reader.Read())
            {
                result = result + $"{cnt}. [{reader[0].ToString()}](tg://user?id={reader[1].ToString()})"+"\n";
                cnt=cnt + 1;
            }
            reader.Close();
            return result;
        }

        private static string SetNickName(long chatid, long userid, string messagetext)
        {
            Console.WriteLine(messagetext.Length<5);
            Console.WriteLine(messagetext.Length);
            if (messagetext.Length > 5)
            {
                if (messagetext.Length < 25)
                {
                    try
                    {
                        if (!messagetext.ToLower().Contains("drop"))
                        {
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

        private static string SetAdminStatus(string admlvl, MyMessage mymessage) 
        {
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

        private static object GetNickname(long chatid, long userid)
        {
            string sql = "SELECT Nickname FROM users WHERE ID =" + chatid.ToString()+userid.ToString();
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            string nickName = (string)cmd.ExecuteScalar();
            return nickName;
        }

        private static string SetDefaultAdmins(long chatid, long userid)
        {
            using (HttpClientHandler hld = new HttpClientHandler())
            {
                using (HttpClient cln = new HttpClient())
                {
                    using (var resp = cln.GetAsync($"https://api.telegram.org/bot" + botToken + $"/getChatAdministrators?chat_id=" + chatid.ToString()).Result)
                    {
                        var json = resp.Content.ReadAsStringAsync().Result;
                        if (!string.IsNullOrEmpty(json))
                        {
                            ChatAdministrators admins = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatAdministrators>(json);
                            if (admins.result != null)
                            {
                                long creatorId = 0;
                                foreach (var admin in admins.result)
                                {
                                    if (admin.status == "creator")
                                    {
                                        creatorId = admin.user.id;
                                    }
                                }
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

        private static string AdminStatus(long chatid, long userid)
        {
            string sql = "SELECT Admin FROM users WHERE ID =" + (chatid.ToString() + userid.ToString());
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            string adminStatus = (string)cmd.ExecuteScalar();
            //Console.WriteLine(adminStatus);
            if (adminStatus!="0")
            {
                return adminStatus;
            }
            else
            {
                using (HttpClientHandler hld = new HttpClientHandler())
                {
                    using (HttpClient cln = new HttpClient())
                    {
                        using (var resp = cln.GetAsync($"https://api.telegram.org/bot" + botToken + $"/getChatAdministrators?chat_id=" + chatid.ToString()).Result)
                        {
                            var json=resp.Content.ReadAsStringAsync().Result;
                            if (!string.IsNullOrEmpty(json))
                            {
                                ChatAdministrators admins = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatAdministrators>(json);
                                if (admins.result != null)
                                {
                                    foreach (var admin in admins.result)
                                    {
                                        if (admin.user.id == userid)
                                        {
                                            sql = $"UPDATE `users` SET `Admin` = '{admin.status}' WHERE `users`.`ID` = {chatid.ToString() + userid.ToString()};";
                                            cmd = new MySqlCommand(sql, conn);
                                            int rowCount = cmd.ExecuteNonQuery();
                                            return admin.status;

                                        }
                                        else
                                        {
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

        private async static void CreateThisUserInDB(long chatid, long userid, string firstName)
        {
            //string sql = $"INSERT INTO `users` (`Number`, `ID`, `Admin`) VALUES (NULL, {message.Chat.Id.ToString()+message.From.Id.ToString()}, '0');";
            string sql = $"INSERT INTO `users` (`Number`, `ID`, `Admin`, `Chat_id`, `Nickname`, `User_ID`,`IsMute`, `Warns`) VALUES (NULL, '{chatid.ToString() + userid.ToString()}', '0', '{chatid}', '{firstName}', '{userid}', '0', '0')";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();
        }

        private static bool isThisUserInDB(long chatid, long userid)
        {
            Console.WriteLine("working");
            //string sql = "SELECT Count(id) FROM chats WHERE id = " + message.Chat.Id;

            string sql = $"SELECT COUNT(id) FROM `users` WHERE id="+chatid.ToString() + userid.ToString();
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

        private static bool isVoiceMessengeBlocked(Telegram.Bot.Types.Message message)
        {
            string sql = "SELECT voiceMessangeBlock FROM chats WHERE ID =" + message.Chat.Id;
            //Console.WriteLine(message.Chat.Id);
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            bool isVoiseBlock =(bool)cmd.ExecuteScalar();
            return isVoiseBlock;

        }

        private static void CreateThisChatInDb(Telegram.Bot.Types.Message message)
        {
            string sql = $"INSERT INTO `chats` (`Number`, `ID`, `IsVIP`, `voiceMessangeBlock`) VALUES (NULL, {message.Chat.Id}, '0', '0');";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();
            //Console.WriteLine("Row Count affected = " + rowCount);
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

        private async static void voiceMessangeCommand(Telegram.Bot.Types.Message message)
        {
            string sql = "SELECT voiceMessangeBlock FROM chats WHERE ID =" + message.Chat.Id;
            //Console.WriteLine(message.Chat.Id);
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