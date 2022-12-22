using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using TgAdmBot.delete;

namespace TgAdmBot.Database
{
    public enum ChatStatus
    {
        free = 0,
        vip = 10
    }
    public enum WarnsLimitAction
    {
        mute = 0,
        none = 10,
        ban = 101,
    }
    public class Chat
    {
        public int ChatId { get; set; }
        public long TelegramChatId { get; set; }
        public int WarnsLimit { get; set; } = 3;
        public string Rules { get; set; } = "Правила ещё не установлены.";
        public int MessagesCount { get; set; } = 0;
        public ChatStatus Status { get; set; } = ChatStatus.free;
        public List<User> Users { get; set; } = new();
        public bool VoiceMessagesDisallowed { get; set; } = false;
        public WarnsLimitAction WarnsLimitAction { get; set; } = WarnsLimitAction.mute;

        public string GetInfo()
        {
            return (
               "📊 Информация о чате:\n"
            + $"📈 ID чата: {TelegramChatId}\n"
            + $"💎 VIP чат: {Status.ToString()}\n"
            + $"🎧 Голосовые сообщения запрещены: {(VoiceMessagesDisallowed ? "Да" : "Нет")}\n"
            + $"⚖️ Наказание за превышение лимита предупреждений: {(WarnsLimitAction == WarnsLimitAction.mute ? "Мут" : "Бан")}\n"
            + $"👨‍💻 Активные пользователи: {Users.Count}\n"
            + $"👨‍💻 Админов: {Users.Where(p=>p.UserRights==UserRights.administrator).Count()}\n"
            + $"✉️ Сообщений всего: {MessagesCount}\n"
                );
        }
        public string GetMutedUsers()
        {
            List<Database.User> mutedUsers = Users.Where(user => user.IsMuted == true).ToList();
            string mutedUsersText = "";
            if (mutedUsers.Count < 1)
            {
                mutedUsersText = "Нет замьюченых пользователей";
            }
            else
            {
                for (int index = 0; index < mutedUsers.Count; index++)
                {
                    mutedUsersText = $"{mutedUsersText}{index+1}. {mutedUsers[index].Nickname}\n";
                }
            }

            return mutedUsersText;
        }
        public string SetWarningLimitAction(long userid, string text)
        {
            if (Users.Single(user => user.TelegramUserId == userid).UserRights < UserRights.moderator)
            {
                string[] command = text.Split(' ').Length > 1 ? text.Split(' ') : new string[] { "", "" };

                switch (command[1])
                {
                    case "mute":
                        WarnsLimitAction = WarnsLimitAction.mute;
                        BotDatabase.db.SaveChanges();
                        return "Теперь после достижения лимита предупреждений пользователю будет запрещено писать";
                    case "ban":
                        WarnsLimitAction = WarnsLimitAction.ban;
                        BotDatabase.db.SaveChanges();
                        return "Теперь после достижения лимита предупреждений пользователь будет удален";
                    default:
                        return "Неизвестный аргумент. Ожидалось mute или ban";
                }
            }
            else
            {
                return "Недостаточно прав!";
            }

        }
        public string SetDefaultAdmins()
        {
            //Request a list of conversation administrators from telegram
            using (HttpClientHandler hld = new HttpClientHandler())
            {
                using (HttpClient cln = new HttpClient())
                {
                    using (var resp = cln.GetAsync($"https://api.telegram.org/bot" + Program.botToken + $"/getChatAdministrators?chat_id=" + TelegramChatId).Result)
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
                                Database.Chat chat = BotDatabase.db.Chats.Single(s => s.TelegramChatId == this.TelegramChatId);
                                chat.Users.Clear();
                                foreach (var admin in admins.result)
                                {
                                    if (admin.status == "creator")
                                    {
                                        creatorId = admin.user.id;
                                        chat.Users.Add(new Database.User { Nickname = admin.user.username, TelegramUserId = admin.user.id, IsBot = admin.user.is_bot, Chat = this, UserRights = UserRights.creator });
                                    }
                                    else if (admin.status== "administrator")
                                    {
                                        chat.Users.Add(new Database.User { Nickname = admin.user.username, TelegramUserId = admin.user.id, IsBot = admin.user.is_bot, Chat = this, UserRights = UserRights.administrator });
                                    }
                                    else
                                    {
                                        chat.Users.Add(new Database.User { Nickname = admin.user.username, TelegramUserId = admin.user.id, IsBot = admin.user.is_bot, Chat = this, UserRights = UserRights.normal });

                                    }
                                }
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
                }
            }
        }

    }
}
