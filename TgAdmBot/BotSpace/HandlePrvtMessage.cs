using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TgAdmBot.Database;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {
        private void HandlePrvtMessage(Message message, Database.Chat chat, Database.User user)
        {
            string msgText = message.Text;
            string modeStr = message.Text.Substring(6, 1);
            string[] users = message.Text.Replace("@", "").Substring(8).Split();
            PrivateMessageModes msgMode = PrivateMessageModes.disallow;

            if (user.TelegramUserId != chat.TelegramChatId)
            {
                //написано не в лс бота
                //Удаление сообщения после сохранения нужной инфы в RAM
                botClient.DeleteMessageAsync(chat.TelegramChatId, message.MessageId);

                //Определение с режимом
                if (modeStr == "1")//allow
                {
                    msgMode = PrivateMessageModes.allow;
                }
                else if (modeStr == "2")//disallow
                {
                    msgMode = PrivateMessageModes.disallow;
                }
                else
                {
                    botClient.SendTextMessageAsync(message.Chat, "Режим указан неверно");
                    return;
                }

                //Формирование сообщения
                List<Database.User> msgUsers = new List<Database.User>();
                int mentionsLen = 8;
                List<MessageEntity> entities = message.Entities.Where(u => u.Type == Telegram.Bot.Types.Enums.MessageEntityType.Mention).ToList();
                for (int i = 0; i < entities.Count; i++)
                {
                    MessageEntity entity = entities[i];
                    if (i != 0 &&
                        Math.Abs(entities[i - 1].Offset + entities[i - 1].Length - entities[i].Offset) > 1)
                    {
                        continue;
                    }
                    else
                    {
                        msgUsers.Add(chat.Users.Single(u => u.TgUsername == users[i]));
                        mentionsLen += 1 + entity.Length;
                    }
                }
                if (!msgUsers.Contains(user))
                {
                    msgUsers.Add(user);
                }
                string mentionsOfUsers = "";
                foreach (Database.User userObj in msgUsers)
                {
                    if (userObj.UserId != user.UserId)
                    {
                        mentionsOfUsers += $"\n[{userObj.Nickname}](tg://user?id={userObj.TelegramUserId})";
                    }
                }
                PrivateMessage newPrivateMsg = new PrivateMessage(msgMode, msgUsers, $"/prvt!{Guid.NewGuid()}", chat.TelegramChatId, msgText.Substring(mentionsLen));
                BotDatabase.db.PrivateMessages.Add(newPrivateMsg);
                InlineKeyboardMarkup markup = new InlineKeyboardMarkup(
                    new InlineKeyboardButton("Показать текст") { CallbackData = newPrivateMsg.Callback }
                    );
                botClient.SendTextMessageAsync(chat.TelegramChatId, $"Скрытое сообщение от [{user.Nickname}](tg://user?id={user.TelegramUserId}){(newPrivateMsg.Mode == PrivateMessageModes.allow ? "Доступно для:" : "Скрыто от:")}\n{mentionsOfUsers}", replyMarkup: markup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

            }
            else
            {
                //     /prvt 1 -28346273482934 @user1 @user2 some text to hide
                //написано в лс бота
                if (modeStr == "1")//allow
                {
                    msgMode = PrivateMessageModes.allow;
                }
                else if (modeStr == "2")//disallow
                {
                    msgMode = PrivateMessageModes.disallow;
                }
                else
                {
                    botClient.SendTextMessageAsync(message.Chat, "Режим указан неверно");
                    return;
                }
                List<Database.User> msgUsers = new List<Database.User>();
                string[] wordsArray = msgText.Split(" ");
                if (wordsArray.Length > 4)
                {
                    users = message.Text.Replace("@", "").Substring(9 + wordsArray[2].Length).Split(" ");
                    int mentionsLen = 8 + wordsArray[2].Length;
                    Database.Chat? selectedChat = BotDatabase.db.Chats.SingleOrDefault(ch => ch.TelegramChatId == Convert.ToInt64(wordsArray[2]));
                    if (selectedChat != null)
                    {
                        List<MessageEntity> entities = message.Entities.Where(u => u.Type == Telegram.Bot.Types.Enums.MessageEntityType.Mention).ToList();
                        for (int i = 0; i < entities.Count; i++)
                        {
                            MessageEntity entity = entities[i];
                            if (i != 0 &&
                                Math.Abs(entities[i - 1].Offset + entities[i - 1].Length - entities[i].Offset) > 1)
                            {
                                continue;
                            }
                            else
                            {
                                Database.User? currentUser = selectedChat.Users.SingleOrDefault(u => u.TgUsername == users[i]);
                                if (currentUser != null)
                                {
                                    msgUsers.Add(currentUser);
                                }
                                mentionsLen += 1 + entity.Length;
                            }
                        }
                        if (!msgUsers.Contains(user) && msgMode == PrivateMessageModes.allow)
                        {
                            msgUsers.Add(user);
                        }
                        else if (msgUsers.Contains(user) && msgMode == PrivateMessageModes.disallow)
                        {
                            msgUsers.Remove(user);
                        }
                        string mentionsOfUsers = "";
                        foreach (Database.User userObj in msgUsers)
                        {
                            if (userObj.UserId != user.UserId)
                            {
                                mentionsOfUsers += $"\n[{userObj.Nickname}](tg://user?id={userObj.TelegramUserId})";
                            }
                        }
                        PrivateMessage newPrivateMsg = new PrivateMessage(msgMode, msgUsers, $"/prvt!{Guid.NewGuid()}", chat.TelegramChatId, msgText.Substring(mentionsLen));
                        BotDatabase.db.PrivateMessages.Add(newPrivateMsg);
                        InlineKeyboardMarkup markup = new InlineKeyboardMarkup(
                            new InlineKeyboardButton("Показать текст") { CallbackData = newPrivateMsg.Callback }
                            );
                        botClient.SendTextMessageAsync(selectedChat.TelegramChatId, $"Скрытое сообщение от [{user.Nickname}](tg://user?id={user.TelegramUserId})\n{(newPrivateMsg.Mode == PrivateMessageModes.allow ? "Доступно для:" : "Скрыто от:")}\n{mentionsOfUsers}", replyMarkup: markup, parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Чат с указанным идентификатором не найден.");
                        return;
                    }

                }
                else
                {
                    botClient.SendTextMessageAsync(message.Chat, "Проверьте синтаксис команды. Он неверен.");
                    return;
                }
            }

        }
    }
}
