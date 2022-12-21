using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgAdmBot.Database
{
    public enum UserRights
    {
        admin = 1,
        user = 2,
        moderator = 4,
        helper = 3,
    }
    public class User
    {
        public int Id { get; set; }
        public int WarnsCount { get; set; } = 0;
        public long TelegramUserId { get; set; }
        public bool IsMuted { get; set; } = false;
        public DateTime LastActivity { get; set; } = DateTime.Now;
        public UserRights UserRights { get; set; } = UserRights.user;
        public List<Chat> Chats { get; set; } = new();
        public string Nickname { get; set; }
        public DateTime UnmuteTime { get; set; } = DateTime.Now;
        public bool IsBot { get; set; } = false;
        public int MessagesCount { get; set; } = 0;
        public int VoiceMessagesCount { get; set; } = 0;
        public int StickerMessagesCount { get; set; } = 0;
        public string LastMessage { get; set; } = "/start";
    }
}
