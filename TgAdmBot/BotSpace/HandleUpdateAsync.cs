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
                //I use two object to work with the messange,
                //since each of them does not implement all the functionality of the second one
                string strupdate = Newtonsoft.Json.JsonConvert.SerializeObject(update);
                Telegram.Bot.Types.Message message = update.Message;

                Database.Chat chat;

                if (BotDatabase.db.Chats.FirstOrDefault(chat => chat.TelegramChatId == message.Chat.Id) == null)
                {
                    BotDatabase.db.Add(new Database.Chat { TelegramChatId = message.Chat.Id, Users = new List<Database.User> { new Database.User { Nickname = message.From.Username, TelegramUserId = message.From.Id, IsBot = message.From.IsBot } } });
                    BotDatabase.db.SaveChanges();
                    chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == message.Chat.Id);
                    chat.SetDefaultAdmins();
                    BotDatabase.db.SaveChanges();
                }

                chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == message.Chat.Id);
                Database.User? user = BotDatabase.db.Users.SingleOrDefault(u => u.Chat.ChatId == chat.ChatId && u.TelegramUserId == message.From.Id);


                if (user == null)
                {
                    chat.Users.Add(new Database.User { Nickname = message.From.Username, TelegramUserId = message.From.Id, IsBot = message.From.IsBot, Chat = chat });
                    BotDatabase.db.SaveChanges();
                    user = chat.Users.Single(user => user.TelegramUserId == message.From.Id);
                }
                if ((message.Voice != null || message.VideoNote != null) & chat.VoiceMessagesDisallowed)
                {
                    await botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                    return;
                }
                #endregion

                chat.MessagesCount += 1;
                BotDatabase.db.SaveChanges();


                if (message.Text != null)
                {
                    Task handledTextTask = this.HandleTextMessage(message, user, chat);
                }

                if (message.Audio!=null)
                {
                    Task handledVoiceTask= this.HandleVoiceMessage(cancellationToken, message);
                }

                user.UpdateStatistic(message);

                #region Хуёвыё код под перепись

                /*if (message.Text != null)
                {
                    
                    
                        if (message.Text.Length > 4)
                        {
                            if (message.Text.ToLower()[0] == 'н' && message.Text.ToLower()[1] == 'и' && message.Text.ToLower()[2] == 'к' && message.Text.ToLower()[3] == ' ')
                            {
                                user.LastMessage = message.Text;
                                BotDatabase.db.SaveChanges();
                                await botClient.SendTextMessageAsync(message.Chat, SetNickname(message.message.chat.id, message.message.from.id, message.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;
                            }
                        }
                        if (message.Text.Length == 9)
                        {
                            if (message.Text.ToLower() == "участники")
                            {
                                user.LastMessage = message.Text;
                                BotDatabase.db.SaveChanges();
                                await botClient.SendTextMessageAsync(message.Chat, GetChatNicknames(message.message.chat.id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                return;
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
                                    await botClient.SendTextMessageAsync(message.Chat, GetRandomNumber(message.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.Length > 10)
                            {
                                if (message.Text.ToLower()[0] == 'в' && message.Text.ToLower()[1] == 'б' && message.Text.ToLower()[2] == 'р' && message.Text.ToLower()[3] == ' ')
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
                                    await botClient.SendTextMessageAsync(message.Chat, Chose(message.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
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
                                        await botClient.SendTextMessageAsync(message.Chat, $"[{GetNickname(message.message.chat.id, message.message.from.id)}](tg://user?id={message.message.from.id}) " + mestext + $" [{GetNickname(message.message.chat.id, message.message.reply_to_message.from.id)}](tg://user?id={message.message.reply_to_message.from.id})", Telegram.Bot.Types.Enums.ParseMode.Markdown);
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
                                    await botClient.SendTextMessageAsync(message.Chat, Who(message.message.text, message.Chat.Id), Telegram.Bot.Types.Enums.ParseMode.Markdown);
                                    return;
                                }
                            }
                            if (message.Text.Length > 5)
                            {
                                if (message.Text.ToLower()[0] == 'в' && message.Text.ToLower()[1] == 'р' && message.Text.ToLower()[2] == 'т' && message.Text.ToLower()[3] == 'н' && message.Text.ToLower()[4] == ' ')
                                {
                                    user.LastMessage = message.Text;
                                    BotDatabase.db.SaveChanges();
                                    await botClient.SendTextMessageAsync(message.Chat, Probability(message.message.text), Telegram.Bot.Types.Enums.ParseMode.Markdown);
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
            }
        }

    }
}
