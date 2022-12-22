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
                Telegram.Bot.Types.Message? message = update.Message;

                Database.Chat chat = Database.Chat.GetOrCreate(message);

                Database.User user = Database.User.GetOrCreate(chat, message);
                #endregion

                chat.MessagesCount += 1;
                BotDatabase.db.SaveChanges();


                if (message.Text != null)
                {
                    Task handledTextTask = this.HandleTextMessage(message, user, chat);
                }
                else if (message.Audio!=null)
                {
                    Task handledVoiceTask= this.HandleVoiceMessage(cancellationToken, message, user, chat);
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
