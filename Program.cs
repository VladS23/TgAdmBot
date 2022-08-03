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
            string sql = "SELECT voiceMessangeBlock FROM chats WHERE ID =" + message.Chat.Id;
            Console.WriteLine(message.Chat.Id);
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            bool isVoiseBlock =(bool)cmd.ExecuteScalar();
            return isVoiseBlock;

        }

        private static void CreateThisChatInDb(Telegram.Bot.Types.Message message)
        {
            string sql = $"INSERT INTO `chats` (`Number`, `ID`, `IsVIP`, `voiceMessangeBlock`) VALUES (NULL, {message.Chat.Id}, '0', '0');";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            int rowCount = cmd.ExecuteNonQuery();
            Console.WriteLine("Row Count affected = " + rowCount);
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
            Console.WriteLine(message.Chat.Id);
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