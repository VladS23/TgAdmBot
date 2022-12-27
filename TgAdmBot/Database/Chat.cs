using System.Text;
using System.Text.RegularExpressions;
using TgAdmBot.BotSpace;

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
        public bool VideoNotesDisallowed { get; set; } = false;
        public bool ObsceneWordsDisallowed { get; set; } = false;

        public Chat()
        {

        }
        public static Chat GetOrCreate(Telegram.Bot.Types.Message message)
        {
            Database.Chat chat;

            if (BotDatabase.db.Chats.FirstOrDefault(chat => chat.TelegramChatId == message.Chat.Id) == null)
            {
                BotDatabase.db.Add(new Database.Chat { TelegramChatId = message.Chat.Id });
                BotDatabase.db.SaveChanges();
                chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == message.Chat.Id);
                chat.SetDefaultAdmins();
                BotDatabase.db.SaveChanges();
            }
            chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == message.Chat.Id);
            return chat;
        }
        public string GetInfo()
        {
            //TODO переделать с использованиеи StringBuilder
            return
               "📊 Информация о чате:\n"
            + $"📈 ID чата: {TelegramChatId}\n"
            + $"⛔️ Лимит предупреждений {WarnsLimit}\n"
            + $"💎 VIP чат: {Status.ToString()}\n"
            + $"🎧 Голосовые сообщения запрещены: {(VoiceMessagesDisallowed ? "Да" : "Нет")}\n"
            + $"⚖️ Наказание за превышение лимита предупреждений: {(WarnsLimitAction == WarnsLimitAction.mute ? "Мут" : "Бан")}\n"
            + $"👨‍💻 Активные пользователи: {Users.Count}\n"
            + $"👨‍💻 Админов: {Users.Where(p => p.UserRights == UserRights.administrator).Count()}\n"
            + $"✉️ Сообщений всего: {MessagesCount}\n"
                ;
        }

        public string GetRanks()
        {
            Database.User owner = Users.Single(u => u.UserRights == UserRights.creator);
            List<Database.User> admins = Users.Where(u => u.UserRights == UserRights.administrator).ToList();
            List<Database.User> moders = Users.Where(u => u.UserRights == UserRights.moderator).ToList();
            List<Database.User> helpers = Users.Where(u => u.UserRights == UserRights.helper).ToList();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Владелец:");
            sb.AppendLine($"[{owner.NicknameMd()}](tg://user?id={owner.TelegramUserId})");
            if (admins.Count > 0)
            {
                sb.AppendLine("\nАдминистраторы:");
                int index = 1;
                foreach (User user in admins)
                {
                    sb.AppendLine($"{index}. [{user.NicknameMd()}](tg://user?id={user.TelegramUserId})");
                    index += 1;
                }
            }
            else
            {
                sb.AppendLine("\nАдминистраторы отсутствуют.");
            }

            if (moders.Count > 0)
            {
                sb.AppendLine("\nМодераторы:");
                int index = 1;
                foreach (User user in moders)
                {
                    sb.AppendLine($"{index}. [{user.NicknameMd()}](tg://user?id={user.TelegramUserId})");
                    index += 1;
                }
            }
            else
            {
                sb.AppendLine("\nМодераторы отсутствуют.");
            }

            if (helpers.Count > 0)
            {
                sb.AppendLine("\nХэлперы:");
                int index = 1;
                foreach (User user in helpers)
                {
                    sb.AppendLine($"{index}. [{user.NicknameMd()}](tg://user?id={user.TelegramUserId})");
                    index += 1;
                }
            }
            else
            {
                sb.AppendLine("\nХэлперы отсутствуют.");
            }
            return sb.ToString();
        }
        public string GetChatNicknames()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Участники беседы:");
            int index = 1;
            foreach (User user in Users)
            {
                sb.AppendLine($"{index}. [{user.NicknameMd()}](tg://user?id={user.TelegramUserId})");
                index += 1;
            }
            return sb.ToString();
        }

        public string GetAllMentions()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Я позвала всех");
            foreach (User user in Users)
            {
                sb.Append($"[ᅠ](tg://user?id={user.TelegramUserId})");
            }
            return sb.ToString();
        }

        public string GetWarnedUsers()
        {
            Database.Chat chat = BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == this.TelegramChatId);
            List<Database.User> users = chat.Users.Where(user => user.WarnsCount > 0).ToList();
            //If warned users are exist than retuln result string else return else string
            if (users.Count > 0)
            {
                //TODO переделать с использованиеи StringBuilder
                string result = "Предупреждённые пользователи:\n";
                int index = 1;
                foreach (Database.User user in users)
                {
                    result = $"{result}{index}. [{user.NicknameMd()}](tg://user?id={user.TelegramUserId})\n";
                    index += 1;
                }
                return result;
            }
            else
            {
                return "Все молодцы! Никто не получал предупреждений";
            }
        }
        public string GetMutedUsers()
        {
            List<Database.User> mutedUsers = Users.Where(user => user.IsMuted == true).ToList();
            //TODO переделать с использованиеи StringBuilder
            string mutedUsersText = "";
            if (mutedUsers.Count < 1)
            {
                mutedUsersText = "Все молодцы, все могут общаться";
            }
            else
            {
                for (int index = 0; index < mutedUsers.Count; index++)
                {
                    mutedUsersText = $"{mutedUsersText}{index + 1}. [{mutedUsers[index].NicknameMd()}](tg://user?id={mutedUsers[index].TelegramUserId}\n";
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
                    //TODO переделать с использованиеи enum в case
                    case "mute":
                        WarnsLimitAction = WarnsLimitAction.mute;
                        BotDatabase.db.SaveChanges();
                        return "Теперь в качестве наказания будет мут";
                    case "ban":
                        WarnsLimitAction = WarnsLimitAction.ban;
                        BotDatabase.db.SaveChanges();
                        return "Теперь в качестве наказания будет бан";
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
            using HttpClientHandler hld = new HttpClientHandler();
            using HttpClient cln = new HttpClient();
            using var resp = cln.GetAsync($"https://api.telegram.org/bot" + Program.botToken + $"/getChatAdministrators?chat_id=" + TelegramChatId).Result;
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
                            chat.Users.Add(new Database.User(admin.user.first_name, admin.user.id, admin.user.is_bot, this) { UserRights = UserRights.creator });
                        }
                        else if (admin.status == "administrator")
                        {
                            chat.Users.Add(new Database.User(admin.user.first_name, admin.user.id, admin.user.is_bot, this) { UserRights = UserRights.administrator });
                        }
                        else
                        {
                            chat.Users.Add(new Database.User(admin.user.first_name, admin.user.id, admin.user.is_bot, this) { UserRights = UserRights.normal });
                        }
                    }
                    BotDatabase.db.SaveChanges();
                    return "Все! Администраторы обновлены!";
                }
                else
                {
                    return "Извини, но тут я послушаюсь только создателя чата";
                }
            }
            else
            {
                return "Ой! У меня что-то не получилось, давай попробуем позднее";
            }
        }
        public string GetMarriages()
        {
            List<Database.User> MarriedUser = Users.Where(user => user.Marriage?.Agreed == true).ToList();
            string result = $"Занятые котики:\n";
            foreach (Database.User user in MarriedUser)
            {
                result = result + $"💖 [{user.NicknameMd()}](tg://user?id={user.TelegramUserId}) и [{user.Marriage.User.NicknameMd()}](tg://user?id={user.Marriage.User.TelegramUserId})\n";
                if (MarriedUser.Count>2)
                {
                    MarriedUser.Remove(MarriedUser.Single(u => u.UserId == user.Marriage.User.UserId));
                }
                else
                {
                    break;
                }
            }
            return result;
        }
    }
}
