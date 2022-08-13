using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using MySql.Data.MySqlClient;
using System.Net.Http;

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


                        if (Array.IndexOf(AdmRangs, AdminStatus(message.Chat.Id, message.From.Id))<=1)
                        {
                            if (message.Text.ToLower().Trim() == "/voicemessange")
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
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, "Недостаточно прав для выполнения данной команды");
                            return;
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
                            return $"Пользователь [Lord](tg://user?id={mymessage.message.from.id}) назначил пользователю @{mymessage.message.reply_to_message.from.username} ранг {admlvl}";
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
            string sql = $"INSERT INTO `users` (`Number`, `ID`, `Admin`, `Chat_id`, `Nickname`) VALUES (NULL, '{chatid.ToString() + userid.ToString()}', '0', '{chatid}', '{firstName}')";
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