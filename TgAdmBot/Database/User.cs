using System.Text.RegularExpressions;
using TgAdmBot.BotSpace;

namespace TgAdmBot.Database
{
    public enum UserRights
    {
        creator = 0,
        administrator = 1,
        moderator = 2,
        helper = 3,
        normal = 4,
    }
    public class User
    {
        public int UserId { get; set; }
        public int WarnsCount { get; set; } = 0;
        public long TelegramUserId { get; set; }
        public bool IsMuted { get; set; } = false;
        public DateTime LastActivity { get; set; } = DateTime.Now;
        public UserRights UserRights { get; set; } = UserRights.normal;
        public Chat Chat { get; set; }
        public string Nickname { get; set; }
        public string? TgUsername { get; private set; }
        public DateTime UnmuteTime { get; set; } = DateTime.Now;
        public bool IsBot { get; set; } = false;
        public int MessagesCount { get; set; } = 0;
        public int VoiceMessagesCount { get; set; } = 0;
        public int StickerMessagesCount { get; set; } = 0;
        public string LastMessage { get; set; } = "/start";
        public Marriage? Marriage { get; set; } = null;

        public User()
        {
        }
        public User(string firstName, long telegramId, bool isBot, Chat chat)
        {
            this.Chat = chat;
            this.IsBot = isBot;
            this.TelegramUserId = telegramId;
            this.Nickname = firstName;
        }
        public string NicknameMd()
        {
            
            return Bot.EscapeMarkdown(Nickname);
        }
        public static User GetOrCreate(Database.Chat chat, Telegram.Bot.Types.User TgUser)
        {
            Database.User? user;
            user = BotDatabase.db.Users.SingleOrDefault(u => u.Chat.ChatId == chat.ChatId && u.TelegramUserId == TgUser.Id);
            if (user == null)
            {
                User newUser = new Database.User
                {
                    Nickname = TgUser.Username != null ? TgUser.Username : TgUser.FirstName,
                    TelegramUserId = TgUser.Id,
                    IsBot = TgUser.IsBot,
                    Chat = chat,
                    TgUsername = TgUser.Username,
                };
                chat.Users.Add(newUser) ;
                BotDatabase.db.SaveChanges();
                user = BotDatabase.db.Users.Single(u => u.Chat.ChatId == chat.ChatId && u.TelegramUserId == TgUser.Id);
                user.TgUsername= TgUser.Username;
                BotDatabase.db.SaveChanges();
                user = BotDatabase.db.Users.Single(u => u.Chat.ChatId == chat.ChatId && u.TelegramUserId == TgUser.Id);
            }
            else
            {
                if (user.TgUsername!=TgUser.Username)
                {
                    user.TgUsername = TgUser.Username;
                    BotDatabase.db.SaveChanges();
                }
            }

            return user;
        }
        public string GetInfo()
        {
            //TODO переделать с использованиеи StringBuilder
            string result =
                $"📈 Информация о {NicknameMd()}\n"
                + $"👥 Ник : [{NicknameMd()}](tg://user?id={TelegramUserId})" + "\n"
                + $"👑 Ранг: {UserRights.ToString()}\n"
                + $"🚫 Количество предупреждений {WarnsCount}\n"
                + $"🖊 Разрешено писать: {(IsMuted ? "Нет" : "Да")}\n";
            if (IsMuted)
            {
                result = result + $"🤐 Замьючен до {UnmuteTime}\n";
            }
            result = result + $"✉️ Количество сообщений: {MessagesCount}\n"
                + $"🎧 Количество голосовых сообщений: {VoiceMessagesCount}\n"
                + $"🕊 В браке: {(Marriage?.Agreed == true ? $"c [{Marriage?.User.NicknameMd()}](tg://user?id={Marriage?.User.TelegramUserId})" : "нет")}\n"
                + $"😀 Количество стикеров: {StickerMessagesCount}";
            return result;
        }
        public void UnBan()
        {
            try
            {
                //Send a request to telegram api and ban user
                using HttpClientHandler hndl = new HttpClientHandler();
                using HttpClient cln = new HttpClient();
                string restext = $"https://api.telegram.org/bot{Program.botToken}/unbanChatMember?user_id={TelegramUserId}&chat_id={Chat.TelegramChatId}&only_if_banned=true";
                using var request = cln.GetAsync(restext).Result;
                //TODO прикрутить сюда приглос чела назад
            }
            catch
            {

            }
        }
        public void Ban()
        {
            try
            {
                using HttpClientHandler hndl = new HttpClientHandler();
                using HttpClient cln = new HttpClient();
                string restext = $"https://api.telegram.org/bot{Program.botToken}/banChatMember?user_id={TelegramUserId}&chat_id={Chat.TelegramChatId}";
                using var request = cln.GetAsync(restext).Result;
            }
            catch
            {

            }
        }
        public void Warn()
        {
            WarnsCount++;
            BotDatabase.db.SaveChanges();
        }
        public string WarningLimitAction()
        {
            switch (Chat.WarnsLimitAction)
            {
                case WarnsLimitAction.ban:
                    Ban();
                    return "Все, доигрался, в бан";
                case WarnsLimitAction.mute:
                    Mute(24 * 60);
                    return "Все! замолчи и подумай о своем поведении!";
                default:
                    return "Кажется что-то пошло не так";
            }
        }
        public void Mute(int minuts)
        {
            try
            {
                IsMuted = true;
                UnmuteTime = DateTime.Now.AddHours(24);
                BotDatabase.db.SaveChanges();
                //Send a request to telegram api and mute user
                using HttpClientHandler hndl = new HttpClientHandler();
                using HttpClient cln = new HttpClient();
                string restext = $"https://api.telegram.org/bot{Program.botToken}/restrictChatMember?user_id={TelegramUserId}&chat_id={Chat.TelegramChatId}&until_date={((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() + (60 * minuts)}";
                using var request = cln.GetAsync(restext).Result;
            }
            catch
            {

            }

        }
        public void Unmute()
        {
            try
            {
                IsMuted = false;
                BotDatabase.db.SaveChanges();

                using HttpClientHandler hndl = new HttpClientHandler();
                using HttpClient cln = new HttpClient();
                string restext = $"https://api.telegram.org/bot{Program.botToken}/restrictChatMember?user_id={TelegramUserId}&chat_id={Chat.TelegramChatId}&until_date={((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds() + 35}";
                //TODO разблочить поток
                using var request = cln.GetAsync(restext).Result;
            }
            catch
            {

            }
        }
        public void UpdateStatistic(Telegram.Bot.Types.Message message)
        {
            IsMuted = false;
            LastActivity = DateTime.Now;
            if (message.Text != null)
            {
                MessagesCount += 1;
                LastMessage = message.Text;
            }
            if (message.Voice != null || message.VideoNote != null)
            {
                VoiceMessagesCount += 1;
            }
            if (message.Sticker != null)
            {
                StickerMessagesCount += 1;
            }
            BotDatabase.db.SaveChanges();
        }
    }
}
