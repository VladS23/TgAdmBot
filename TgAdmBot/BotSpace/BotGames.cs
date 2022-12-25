using TgAdmBot.Database;

namespace TgAdmBot.BotSpace
{
    //TODO переделать нейминг переменных методов
    internal class BotGames
    {
        public static string GetRandomNumber(string messagetext)
        {
            try
            {
                //TODO переделать с использованиеи нормального рандома

                string mes = messagetext.Substring(4);
                string[] nums = mes.Split('-');
                Random rnd = new Random();
                return "🎲🎲 Я бросила кости и выпало " + rnd.Next(Convert.ToInt32(nums[0]), Convert.ToInt32(nums[1]));
            }
            catch
            {
                return "Я тебя не поняла(. Пример правильной команды 'рнд 1-12'";
            }
        }
        public static string Chose(string messagetext)
        {
            try
            {
                //TODO переделать с использованиеи нормального рандома

                string mes = messagetext.Substring(4);
                string[] separator = { " или " };//TODO убрать
                string[] variables = mes.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                Random rnd = new Random();
                return "✨✨ Ух ты! правда мне можно выбрать? Спасибо за доверие! Я выбираю " + variables[rnd.Next(0, 2)];
            }
            catch
            {
                return "Я тебя не поняла(. Пример правильной команды '/chs вариант 1 или вариант 2'";
            }
        }
        public static string Who(string text, Chat chat)
        {
            //TODO переделать с использованиеи нормального рандома

            text = text.Substring(4);
            Database.User user = chat.Users[new Random().Next(0, chat.Users.Count - 1)];
            return $"[{user.FirstName}](tg://user?id={user.TelegramUserId}) {text}";
        }
        public static string Probability(string messagetext)
        {
            //TODO переделать с использованиеи нормального рандома

            string mes = messagetext.Substring(5);
            Random rnd = new Random();
            return "Вероятность" + mes + $" {rnd.Next(0, 101)}%";
        }
    }
}
