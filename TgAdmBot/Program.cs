using Telegram.Bot;
using TgAdmBot.VoskRecognition;

namespace TgAdmBot
{
    internal class Program
    {
        public static string botToken = new Config().env.GetValueOrDefault("BotToken")!;
        public static string ownerId = new Config().env.GetValueOrDefault("OwnerId")!;
        public static string dbFileName = new Config().env.GetValueOrDefault("DatabaseFileName")!;

        private static void Main(string[] args)
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