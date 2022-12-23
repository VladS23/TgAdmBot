using Microsoft.EntityFrameworkCore;

namespace TgAdmBot.Database
{
    public class BotDatabase
    {
        public static Database db = null;
        public BotDatabase()
        {
            if (db == null)
            {
                db = new Database();

                //Для автосброса БД в отладочной сборке
#if (DEBUG)
                db.Database.EnsureDeleted();//TODO ознакомиться (!)
#endif

                db.Database.EnsureCreated();
                db.Chats.Load();
                db.Users.Load();
            }
        }
        public class Database : DbContext
        {
            public Database() : base()
            {

            }
            public DbSet<User> Users { get; set; }
            public DbSet<Chat> Chats { get; set; }
            public DbSet<VoiceMessage> VoiceMessages { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder options)
            {
                options.UseSqlite(@"Data Source=database.db");
            }

        }
    }
}
