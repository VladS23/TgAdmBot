using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

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
        public DateTime UnmuteTime { get; set; } = DateTime.Now;
        public bool IsBot { get; set; } = false;
        public int MessagesCount { get; set; } = 0;
        public int VoiceMessagesCount { get; set; } = 0;
        public int StickerMessagesCount { get; set; } = 0;
        public string LastMessage { get; set; } = "/start";

        public string GetInfo()
        {
            return (
                $"📈 Информация о {Nickname}\n"
                + $"👥 Ник : [{Nickname}](tg://user?id={TelegramUserId})" + "\n"
                + $"👑 Ранг: {UserRights.ToString()}\n"
                );
        }

        public void UpdateStatistic(Telegram.Bot.Types.Message message)
        {
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
            BotDatabase.db.Chats.Single(chat => chat.TelegramChatId == message.Chat.Id).MessagesCount += 1;
            BotDatabase.db.SaveChanges();
        }
    }
}
