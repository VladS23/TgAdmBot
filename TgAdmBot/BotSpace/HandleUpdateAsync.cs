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



                #region Хуёвыё код под перепись

                /*if (message.Text != null)
                {
                    

                        if (message.Text.Length == 9)
                        {
                            if (message.Text.ToLower() == "участники")
                            {

                            }
                        }
                        if (Array.IndexOf(AdmRangs, AdminStatus(message.message.chat.id, message.message.from.id)) <= 4)
                        {
                            if (message.Text.Length > 6)
                            {
                                if (message.Text.ToLower()[0] == 'р' && message.Text.ToLower()[1] == 'н' && message.Text.ToLower()[2] == 'д' && message.Text.ToLower()[3] == ' ')
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
                                    botClient.SendTextMessageAsync(message.Chat, GetRandomNumber(message.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.Length > 10)
                            {
                                if (message.Text.ToLower()[0] == 'в' && message.Text.ToLower()[1] == 'б' && message.Text.ToLower()[2] == 'р' && message.Text.ToLower()[3] == ' ')
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
                                    botClient.SendTextMessageAsync(message.Chat, Chose(message.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.Length > 3)
                            {
                                if (message.Text.ToLower()[0] == 'm' && message.Text.ToLower()[1] == 'e' && message.Text.ToLower()[2] == ' ')
                                {
                                    if (message.message.reply_to_message != null)
                                    {
                                        user.LastMessage = message.Text;
                                        BotDatabase.db.SaveChanges();
                                        string mestext = message.message.text.Substring(3);
                                        botClient.SendTextMessageAsync(message.Chat, $"[{GetNickname(message.message.chat.id, message.message.from.id)}](tg://user?id={message.message.from.id}) " + mestext + $" [{GetNickname(message.message.chat.id, message.message.reply_to_message.from.id)}](tg://user?id={message.message.reply_to_message.from.id})", Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                        botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
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
                                    botClient.SendTextMessageAsync(message.Chat, Who(message.message.text, message.Chat.Id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.Length > 5)
                            {
                                if (message.Text.ToLower()[0] == 'в' && message.Text.ToLower()[1] == 'р' && message.Text.ToLower()[2] == 'т' && message.Text.ToLower()[3] == 'н' && message.Text.ToLower()[4] == ' ')
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
                                    botClient.SendTextMessageAsync(message.Chat, Probability(message.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                        }
                        //Administrative command
                        if (message.Text.ToLower()[0] == '/')
                        {





                        }
                    }
                    //Deletes voice messages if they are blocked

                }*/
                #endregion

                #region Хуёвый код под перепись
                /*
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
return result;
                }
                //Set user's Nickname
                private static string SetNickname(long chatid, long userid, string messagetext)
                {
                    //Checking the validity of the nickname

                //Buld message about random user from chat
                private static string Who(string text, long chatid)
                {
                    Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == chatid);
                    Database.User user = chat.Users[new Random().Next(0, chat.Users.Count - 1)];
                    return $"[{user.Nickname}](tg://user?id={user.TelegramUserId}) {text}";
                }
                //Build message about users activity
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
                //Builds a message about muted user
                //**************
                //Ban commands
                //**************
                //Add user in the ban
                //Removing a user from the chat blacklist
                //**************
                //Mute commands
                //**************
                //Prohibit the user from writing messages
                //Allow the user to write messages
                private void UpdateUserStatistic(Telegram.Bot.Types.Message message)
                {
                        //Update last activity
                        user.LastActivity = DateTime.Now;
                        if (message.Text != null)
                        {
                            //Update message count
                            user.MessagesCount += 1;
                        }
                        if (message.Voice != null || message.VideoNote != null)
                        {
                            user.VoiceMessagesCount += 1;
                        }
                        if (message.Sticker != null)
                        {
                            //Update StikerCount
                            user.StickerMessagesCount += 1;
                        }
                        BotDatabase.db.SaveChanges();
                    }
                }
                */
                #endregion
            }
        }

    }
}
