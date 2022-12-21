using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int Id { get; set; }
        public int WarnsCount { get; set; } = 0;
        public long TelegramUserId { get; set; }
        public bool IsMuted { get; set; } = false;
        public DateTime LastActivity { get; set; } = DateTime.Now;
        public UserRights UserRights { get; set; } = UserRights.normal;
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
