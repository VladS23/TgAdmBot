using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using MySql.Data.MySqlClient;

namespace TgAdmBot
{
    class Program
    {
        private static ITelegramBotClient bot = new TelegramBotClient("5328960378:AAH1fskxZcH3GEXMVKHROuhfnNJQsSj8gvU");
        private static MySqlConnection conn = new MySqlConnection("server=localhost; port=3306; username=root; password=root; database=tgadmbot");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (isThisChatInDB(message))
                {
                    Console.WriteLine(1);
                    if (message.Text != null)
                    {
                        Console.WriteLine(message.From.Id);
                        if (message.Text.ToLower()[0] == '/')
                        {
                            if (message.Text.ToLower().Trim() == "/voicemessange")
                            {
                                voiceMessangeCommand(message);
                                if (isVoiceMessengeBlocked(message)==true)
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
                                await botClient.SendTextMessageAsync(message.Chat, "Неизвестная комманда");
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
                else
                {
                    CreateThisChatInDb(message);
                }
            }
        }

        private static bool isVoiceMessengeBlocked(Telegram.Bot.Types.Message message)
        {
            string sql = "SELECT * FROM chats WHERE ID =" + message.Chat.Id;
            Console.WriteLine(message.Chat.Id);
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            string isVoiseBlock = "false";
            while (reader.Read())
            {
                isVoiseBlock = reader[3].ToString();
            }
            reader.Close();
            if (isVoiseBlock == "true")
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        private static void CreateThisChatInDb(Telegram.Bot.Types.Message message)
        {
            string sql = "Insert chats (ID, IsVIP, voiceMessangeBlock) "
                                              + " values (@ID, @IsVIP, @voiceMessangeBlock) ";

            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;

            // Создать объект Parameter.
            MySqlParameter gradeParam = new MySqlParameter();
            cmd.Parameters.Add("@ID", MySqlDbType.VarChar).Value = message.Chat.Id;
            cmd.Parameters.Add("@IsVIP", MySqlDbType.VarChar).Value = "false";
            cmd.Parameters.Add("@voiceMessangeBlock", MySqlDbType.VarChar).Value = "false";

            // Выполнить Command (Используется для delete, insert, update).
            int rowCount = cmd.ExecuteNonQuery();

            Console.WriteLine("Row Count affected = " + rowCount);
        }

        private static bool isThisChatInDB(Telegram.Bot.Types.Message message)
        {
            string sql = "SELECT Count(id) FROM chats WHERE id = " + message.Chat.Id;
            bool isThisChatInDB = false;
            string req = "";
            Console.WriteLine(message.Chat.Id);
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                req = reader[0].ToString();
            }
            if (req == "1")
            {
                isThisChatInDB = true;
            }
            Console.WriteLine(req);
            reader.Close();
            return isThisChatInDB;

        }

        private async static void voiceMessangeCommand(Telegram.Bot.Types.Message message)
        {
            string sql = "SELECT * FROM chats WHERE ID =" + message.Chat.Id;
            Console.WriteLine(message.Chat.Id);
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader reader = cmd.ExecuteReader();
            string isVoiseBlock = "false";
            while (reader.Read())
            {
                isVoiseBlock = reader[3].ToString();
            }
            reader.Close();
            sql = "Update chats set voiceMessangeBlock	 = @voiceMessangeBlock	 where ID = @ID	";
            cmd.Connection = conn;
            cmd.CommandText = sql;
            if (isVoiseBlock == "true")
            {
                cmd.Parameters.Add("@voiceMessangeBlock", MySqlDbType.VarChar).Value = "false";
                cmd.Parameters.Add("@ID", MySqlDbType.VarChar).Value = message.Chat.Id;
                int rowCount = cmd.ExecuteNonQuery();
            }
            if (isVoiseBlock == "false")
            {
                cmd.Parameters.Add("@voiceMessangeBlock", MySqlDbType.VarChar).Value = "true";
                cmd.Parameters.Add("@ID", MySqlDbType.VarChar).Value = message.Chat.Id;
                int rowCount = cmd.ExecuteNonQuery();

            }
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
                AllowedUpdates = { }, // receive all update types
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