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
using TgAdmBot.IVosk;

namespace TgAdmBot
{
    class Program
    {
        public static string botToken = new Config().env.GetValueOrDefault("BotToken")!;
        static void Main(string[] args)
        {
            Vosk.Vosk.SetLogLevel(0);
            SpeechRecognizer.voskRecognizer.SetMaxAlternatives(0);
            SpeechRecognizer.voskRecognizer.SetWords(true);
            BotSpace.Bot bot = new BotSpace.Bot();
            Console.WriteLine(bot.botClient.GetMeAsync());
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}