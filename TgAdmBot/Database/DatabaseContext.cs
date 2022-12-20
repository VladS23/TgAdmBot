using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgAdmBot.Database
{
    public class BotDatabase
    {
        public static Database? db = null;
        public BotDatabase()
        {
            if (db == null)
            {
                db = new Database();
                db.Database.EnsureCreated();
            }
        }
        public class Database : DbContext
        {
            public DbSet<User> Users { get; set; }
            public DbSet<Chat> Chats { get; set; }
            protected override void OnConfiguring(DbContextOptionsBuilder options)
            {
                options.UseSqlite($"Data Source=database.db");
            }
        }
    }
}
