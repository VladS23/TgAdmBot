using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgAdmBot.Database
{
    internal class BotGames
    {
        public static string GetRandomNumber(string messagetext)
        {
            try
            {
                //Process the string and create a result based on it
                string mes = messagetext.Substring(4);
                string[] nums = mes.Split('-');
                Random rnd = new Random();
                return "🎲🎲 Я бросил кости и выпало " + rnd.Next(Convert.ToInt32(nums[0]), Convert.ToInt32(nums[1]));
            }
            catch
            {
                return "Неправильный синтаксис команды. Пример правильной команды 'рнд 1-12'";
            }
        }
        public static string Chose(string messagetext)
        {
            try
            {
                string mes = messagetext.Substring(4);
                string[] separator = { " или " };
                string[] variables = mes.Split(separator, System.StringSplitOptions.RemoveEmptyEntries);
                Random rnd = new Random();
                return "✨✨ Я выбираю " + variables[rnd.Next(0, 2)];
            }
            catch
            {
                return "Неправильный синтаксис команды. Пример правильной команды '/chs вариант 1 или вариант 2'";
            }
        }
        public static string Who(string text, Chat chat) 
        {
            text = text.Substring(4);
            Database.User user = chat.Users[new Random().Next(0, chat.Users.Count - 1)];
            return $"[{user.Nickname}](tg://user?id={user.TelegramUserId}) {text}";
        }
        public static string Probability(string messagetext)
        {
            string mes = messagetext.Substring(5);
            Random rnd = new Random();
            return "Вероятность" + mes + $" {rnd.Next(0, 101)}%";
        }
    }
}
