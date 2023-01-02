using Microsoft.EntityFrameworkCore;

namespace TgAdmBot.Database
{
    public class BotDatabase
    {
        public static DatabaseContext db = null;
        public BotDatabase()
        {
            if (db == null)
            {
                db = new DatabaseContext();

                db.Database.EnsureCreated();
                db.Chats.Load();
                db.Users.Load();
            }
        }
    }

    public class DatabaseContext : DbContext
    {
        public DatabaseContext() : base()
        {
            this.Database.EnsureCreated();
            this.SaveChangesFailed += SaveFailed;
        }

        public void SaveFailed(object obj, SaveChangesFailedEventArgs failedEventArgs)
        {
            Console.WriteLine(failedEventArgs);
        }



        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<VoiceMessage> VoiceMessages { get; set; }
        public DbSet<Marriage> Marriages { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }
        public DbSet<Billing> Billings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var di = new DirectoryInfo(Directory.GetCurrentDirectory());
            var root = di.Parent;
            var dbPath = root != null ? root.ToString() : Directory.GetCurrentDirectory();
            options.UseSqlite($"Data Source={Path.Combine(dbPath, Program.dbFileName)}");
        }

    }
}
