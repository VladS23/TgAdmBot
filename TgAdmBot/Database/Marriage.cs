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
