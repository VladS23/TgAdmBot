﻿namespace TgAdmBot.BotSpace
{

    internal class BotPhrases
    {
        public const string OldMessagePh = "Похоже это было до того как я пришла, я не понмаю о чем ты(";
        public const string ReplyToMesage = "Я не понимаю о чем ты, ответь, пожалуйста, на сообщение, которое хочешь мне показать";
        public const string HelpMessage = "О чем бы ты хотел узнать? \n" +
                                              "1. Развлечения \n" +
                                              "2. Настройка беседы\n" +
                                              "3. Администрирование";
        public const string HelpMessageFunActions =
        "Список развлекательных команд\n" +
                            "1. /nick + имя - установит тебе в качестве ника \"имя\"\n" +
                            "2. /nicks - выведет список всех участников беседы \n" +
                            "3. /stat - выведет твою статистику, пользователи с рангом модератор и выше могут посмотреть статистику пользователей с рангом меньше, чем у них, если напишут это сообщение в ответ на сообщение пользователя, статистику которого необходимо просмотреть\n" +
                            "4. /rnd число1-число2 - сгенерирует случайное число из указанного промежутка\n" +
                            "5. /chs вариант1 или вариант2 - выберет один из указанных вариантов\n" +
                            "6. /me действие - выведет сообщение вида: \"пользователь1 действие пользователь2\" (пользователь2 указывается путем ответа на его сообщение) пользователя указывать необязательно\n" +
                            "7. /wh + действие - выведет: *Случайный участник беседы* действие\n" +
                            "8. /prob + событие - предположит вероятность какого-то события\n" +
                            "8. /stt в ответ на голосовое сообщение или видеозаметку (кружок с видео) отправит его текстовую расшифровку\n" +
                            "9. /chatstat выведет статистику чата\n" +
                            "10. /stt перевести указанное голосовое или видео сообщение в текст\n" +
                            "11. /rules вывести правила беседы\n" +
                            "12. /marry заключить брак\n" +
                            "13. /marriages вывести браки беседы\n" +
                            "14. /prvt поможет создать секретное сообщение. Синтаксис:\n/prvt 1 @username @seconduser текст который будет скрыт\nЧисло через пробел - это режим. 1 - разрешающий, 2 - запрещающий. Список пользователей, которым будет разрешено (в 1 режиме) и запрещено (во 2 режиме) указывается через пробел после указания режима. Далее следует текст, который будет скрыт.\n" +
                            "15. /divorce разорвать брак";
        public const string HelpMessageSettingsActions =
        "Список доступных настроек беседы\n" +
                            "1. /setdefaultadmins - назначит администраторов беседы в соответсвтвие с тем, как расставлены права в телеграм, может использоваться только создателем беседы\n" +
                            "2. /voicemessage заблокирует или разблокирует голосовые и видеосообщения, по умолчанию разблокировано, может применяться администраторами или создателем\n" +
                            "3. /setwarninglimitaction - установка наказания за превышение количества предупреждений, по умолчанию mute\n" +
                            "4. /setrules - установка правил\n" +
                            "5. /silientmode - включение режима тишины\n" +
                            "6. /filterwords - включить/выключить фильтр мата";
        public const string HelpMessageAdminActions =
        "Список доступных административных действий:\n" +
                            "1. Назначения ранга пользователям. Ранг создателя сразу выдается создателем беседы, остальные ранги могут быть назначены пользователем с рангом выше. Чтобы назначить ранг необходимо написать одну из следующих команд в ответ на сообщение пользователя, которому необходимо назначить ранг\n   /admin\n    /moder\n    /helper\n    /normal\n" +
                            "2. /mute Если ваш ранг выше или равен модератору и выше, чем у пользователя, на сообщение, которого вы ответили, то вы запретите или разрешите ему писать сообщения, не работает на пользователей, которым в настройках беседы телеграм установлен ранг администратор\n" +
                            "3. /muted Выведет список всех, кому запрещено писать сообщения\n" +
                            "4. /ban Исключит пользователя из беседы и добавит его в черный список чата, данная команда доступна только создателю и администратора, не работает на пользователей, котрым в настройках беседы в телеграм установлен ранг администратора\n" +
                            "5. /unban  Исключит пользователя из черного списка чата, команда доступна администраторам и создателям\n" +
                            "6. /ranks  Выведет список пользователей с повышенными правами\n" +
                            "7. /warn Выдаст пользователю предупреждение, по достижению трех предупреждений он будет исключен из беседы, команда доступна создателю или администраторам\n" +
                            "8. /silientmode Запретит писать сообщения всем пользователям, кроме указанных администраторами в настройках чата телеграмм \n" +
                            "9. /warns Выведет список предупрежденных пользователей беседы, доступна создателю или администраторам \n" +
                            "10. /unwarn Снимет с пользователя все предупреждения";
        public const string NextChatRules = "Отлично! А теперь скажи мне какие правила нужны";
        public const string setRights = "Окей, записала, теперь он ";
        public const string NotEnoughtRights = "Прости, но ты не можешь делать такие громкие заявления(";
        public const string ShortMessage = "Кажется ты меня обманываешь... Как-то слишком коротко...";
        public const string StartCommandAnswer = "Приветик!\nМеня зовут Амалия, пупсик)\nДобавляй меня в свою беседу и пиши /help\nА там.. разберёшься)";
    }
}
