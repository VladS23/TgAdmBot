using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgAdmBot.Database
{
    public enum PrivateMessageModes
    {
        allow,
        disallow
    }
    public class PrivateMessage
    {
        public int PrivateMessageId { get; set; }
        public string Text { get; set; }
        public PrivateMessageModes Mode { get; set; }
        public List<Database.User> Users { get; private set; }
        public string Callback { get; set; }
        public long ChatId { get; set; }
        public PrivateMessage()
        {

        }
        public PrivateMessage(PrivateMessageModes mode, List<User> users, string callback,long chatId,string text)
        {
            Text= text;
            Users = users;
            Callback = callback;
            ChatId = chatId;
            Mode = mode;
        }
    }
}
