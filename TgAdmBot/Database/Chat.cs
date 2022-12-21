using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int Id { get; set; }
        public long TelegramChatId { get; set; }
        public int WarnsLimit { get; set; } = 3;
        public string Rules { get; set; } = "Правила ещё не установлены.";
        public int MessagesCount { get; set; } = 0;
        public ChatStatus Status { get; set; } = ChatStatus.free;
        public List<User> Users { get; set; } = new();
        public bool VoiceMessagesDisallowed { get; set; } = false;
        public WarnsLimitAction WarnsLimitAction { get; set; } = WarnsLimitAction.mute;

    }
}
