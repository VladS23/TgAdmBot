namespace TgAdmBot.BotSpace
{
    internal class ChatAdministrators
    {
        public bool ok { get; set; }
        public Result[] result { get; set; }
    }

    public class Result
    {
        public User user { get; set; }
        public string status { get; set; }
        public bool can_be_edited { get; set; }
        public bool can_manage_chat { get; set; }
        public bool can_change_info { get; set; }
        public bool can_delete_messages { get; set; }
        public bool can_invite_users { get; set; }
        public bool can_restrict_members { get; set; }
        public bool can_pin_messages { get; set; }
        public bool can_promote_members { get; set; }
        public bool can_manage_video_chats { get; set; }
        public bool is_anonymous { get; set; }
        public bool can_manage_voice_chats { get; set; }
    }

    public class User
    {
        public long id { get; set; }
        public bool is_bot { get; set; }
        public string first_name { get; set; }
        public string? username { get; set; }
        public string language_code { get; set; }
    }

}