namespace TgAdmBot
{
    internal class MyMessage
    {

        public int update_id { get; set; }
        public Message message { get; set; }
    }

    public class Message
    {
        public int message_id { get; set; }
        public From from { get; set; }
        public int date { get; set; }
        public Chat chat { get; set; }
        public Reply_To_Message reply_to_message { get; set; }
        public string text { get; set; }
    }

    public class From
    {
        public int id { get; set; }
        public bool is_bot { get; set; }
        public string first_name { get; set; }
        public string username { get; set; }
        public string language_code { get; set; }
    }

    public class Chat
    {
        public long id { get; set; }
        public string type { get; set; }
        public string title { get; set; }
    }

    public class Reply_To_Message
    {
        public int message_id { get; set; }
        public From1 from { get; set; }
        public int date { get; set; }
        public Chat1 chat { get; set; }
        public string text { get; set; }
    }

    public class From1
    {
        public int id { get; set; }
        public bool is_bot { get; set; }
        public string first_name { get; set; }
        public string username { get; set; }
        public string language_code { get; set; }
    }

    public class Chat1
    {
        public long id { get; set; }
        public string type { get; set; }
        public string title { get; set; }
    }
}
