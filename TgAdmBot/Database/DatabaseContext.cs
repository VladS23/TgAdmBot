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

                //Для автосброса БД в отладочной сборке
#if DEBUG
                //db.Database.EnsureDeleted();//TODO ознакомиться (!)
#endif

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

        }
        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<VoiceMessage> VoiceMessages { get; set; }
        public DbSet<Marriage> Marriages { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={Program.dbFileName}");
        }

    }
}
