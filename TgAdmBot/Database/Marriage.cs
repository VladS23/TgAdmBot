using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TgAdmBot.Database;

namespace TgAdmBot.Database
{
    public class Marriage
    {
        public int MarriageId { get; set; }
        public DateTime DateOfConclusion { get; set; } = DateTime.Now;
        public int UserId { get; }
        public User User { get; }
        public bool Agreed { get; set; } = false;

        public Marriage() { }
        public Marriage(User user)
        {

            this.User = user;
            this.UserId = user.UserId;
        }
    }
}
