using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TgAdmBot.Database;

namespace TgAdmBot.Database
{
    internal class Marriage
    {
        public int MarriageId { get; set; }
        public long TelegramChatId { get; set; }
        public DateTime DateOfConclusion { get; set; } = DateTime.Now;
        public User User1 { get; set; }
        public User User2 { get; set; }

        public Marriage(long telegramChatId, DateTime dateOfConclusion, User user1, User user2)
        {
            TelegramChatId = telegramChatId;
            DateOfConclusion = dateOfConclusion;
            User1 = user1;
            User2 = user2;
            BotDatabase.db.Marriages.Single(m => m.TelegramChatId == telegramChatId).Marriages.Add(new Database.Marriage
            {
                TelegramChatId = telegramChatId;
                DateOfConclusion = dateOfConclusion;
                User1 = user1;
                User2 = user2;
        });
            BotDatabase.db.SaveChanges();
        }
    }
}
