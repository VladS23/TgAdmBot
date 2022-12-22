using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using TgAdmBot.Database;
using TgAdmBot.BotSpace;

namespace TgAdmBot
{
    class Program
    {
        public static string botToken = new Config().env.GetValueOrDefault("BotToken")!;
        static void Main(string[] args)
        {
            BotSpace.Bot bot = new BotSpace.Bot();
            Console.WriteLine(bot.botClient.GetMeAsync());
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}