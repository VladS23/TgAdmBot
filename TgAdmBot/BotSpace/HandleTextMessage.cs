using Telegram.Bot;
using Telegram.Bot.Types;
using TgAdmBot.Database;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {

        private async Task HandleTextMessage(Telegram.Bot.Types.Message message, Database.User user, Database.Chat chat)
        {
            Database.User? replUser = chat.Users.SingleOrDefault(u => u.TelegramUserId == message.ReplyToMessage?.From?.Id);
            if (replUser == null && message.ReplyToMessage != null)
            {
                replUser = Database.User.GetOrCreate(chat, message.ReplyToMessage.From);
            }





            if (message.Text.Contains("@all"))
            {
                botClient.SendTextMessageAsync(message.Chat, chat.GetAllMentions(), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
            }
            //List<Database.User> MarriedUser = chat.Users.Where(user => user.Marriage?.Agreed == true).ToList();
            switch (message.Text.Replace($"@{botClient.GetMeAsync().Result.Username!}", "").ToLower().Split()[0])
            {
                case "/marriages":
                    {
                        botClient.SendTextMessageAsync(message.Chat, chat.GetMarriages(), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                        break;
                    }
                case "/marry":
                    if (replUser != null)
                    {
                        if (replUser.Marriage?.Agreed==true)
                            replUser.Marriage.Agreed = false;
                        
                        Marriage marriage = new Marriage(replUser);
                        user.Marriage = marriage;
                        BotDatabase.db.SaveChanges();
                        if (replUser.Marriage?.User == user)
                        {
                            if (replUser.Marriage.User != user.Marriage.User)
                            {
                                replUser.Marriage.Agreed = true;
                                user.Marriage.Agreed = true;
                                BotDatabase.db.SaveChanges();
                                //List<Database.User> MarriedUser = chat.Users.Where(user => user.Marriage?.Agreed == true).ToList();
                                //TODO отправка свидетельств
                                botClient.SendTextMessageAsync(message.Chat, $"Желаю Вам, [{user.NicknameMd()}](tg://user?id={user.TelegramUserId}) и [{replUser.NicknameMd()}](tg://user?id={replUser.TelegramUserId}) счастливого брака\\!", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, $"Прости, солнце, я понимаю, что ты у мамы самый лучший, но провести бракосочетание в одиночку не получится.", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            }

                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, $"Теперь [{replUser.NicknameMd()}](tg://{replUser.TelegramUserId}) должен ответить твое сообщение /marry", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, $"Нет, это так не работает, прежде чем заключать брак необходимо кого-нибудь себе найти", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);

                    }
                    break;
                case "/filterwords":
                    chat.ObsceneWordsDisallowed = !chat.ObsceneWordsDisallowed;
                    BotDatabase.db.SaveChanges();
                    if (chat.ObsceneWordsDisallowed)
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Хорошо, солнышко, буду удалять маты из этого чатика✨✨");
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Ю-хуу!\nДаёшь свободу слова! Теперь я не буду следить за вашими выражениями)");
                    }
                    break;
                case "/silientmode":
                    if (user.UserRights < UserRights.moderator)
                    {
                        botClient.SendTextMessageAsync(message.Chat, "Выберите команду:\n1. Включить\n2. Выключить");
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights);
                    }
                    break;
                case "/start":
                    botClient.SendTextMessageAsync(message.Chat, BotPhrases.StartCommandAnswer);
                    break;
                case "/ranks":
                    botClient.SendTextMessageAsync(message.Chat, chat.GetRanks(), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                    break;
                case "/stt":
                    if (replUser != null)
                    {
                        VoiceMessage? voiseMessage = BotDatabase.db.VoiceMessages.SingleOrDefault(vm => vm.Chat.ChatId == chat.ChatId && vm.MessageId == message.ReplyToMessage.MessageId);
                        if (voiseMessage == null)
                        {
                            botClient.SendTextMessageAsync(chatId: message.Chat, BotPhrases.OldMessagePh);
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(chatId: message.Chat, voiseMessage.recognizedText, replyToMessageId: message.ReplyToMessage.MessageId);
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.ReplyToMesage);
                    }
                    break;
                case "/db":
                    if (message.Chat.Id == Convert.ToInt64(Program.ownerId))
                    {
                        //TODO отправка бд в чат   
                    }
                    break;
                case "/help":
                    botClient.SendTextMessageAsync(message.Chat, BotPhrases.HelpMessage);
                    break;
                case "1":
                    if (user.LastMessage == "/help")
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.HelpMessageFunActions);
                    }
                    else if (user.LastMessage.StartsWith("/silientmode") && user.UserRights < UserRights.moderator)
                    {
                        ChatPermissions silientPermissions = new ChatPermissions();
                        silientPermissions.CanSendMessages = false;
                        silientPermissions.CanSendPolls = false;
                        silientPermissions.CanSendOtherMessages = false;
                        silientPermissions.CanPinMessages = false;
                        silientPermissions.CanSendMediaMessages = false;
                        silientPermissions.CanSendPolls = false;
                        silientPermissions.CanChangeInfo = false;
                        botClient.SetChatPermissionsAsync(chatId: message.Chat, silientPermissions);
                        botClient.SendTextMessageAsync(message.Chat, "Тихий режим активировала, котик");
                    }
                    break;
                case "2":
                    if (user.LastMessage == "/help")
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.HelpMessageSettingsActions);
                    }
                    else if (user.LastMessage.StartsWith("/silientmode") && user.UserRights < UserRights.moderator)
                    {
                        ChatPermissions silientPermissions = new ChatPermissions();
                        silientPermissions.CanSendMessages = true;
                        silientPermissions.CanSendPolls = false;
                        silientPermissions.CanSendOtherMessages = true;
                        silientPermissions.CanPinMessages = false;
                        silientPermissions.CanSendMediaMessages = true;
                        silientPermissions.CanSendPolls = true;
                        silientPermissions.CanChangeInfo = false;
                        botClient.SetChatPermissionsAsync(chatId: message.Chat, silientPermissions);
                        botClient.SendTextMessageAsync(message.Chat, "Тихий режим деактивировала, котик");
                    }
                    break;
                case "3":
                    if (user.LastMessage == "/help")
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.HelpMessageAdminActions);
                    }
                    break;
                case "/chatstat":

                    BotDatabase.db.SaveChanges();
                    botClient.SendTextMessageAsync(message.Chat, chat.GetInfo());
                    break;
                case "/setdefaultadmins":
                    if (user.UserRights == UserRights.creator)
                    {
                        BotDatabase.db.SaveChanges();
                        botClient.SendTextMessageAsync(message.Chat, chat.SetDefaultAdmins());
                        break;
                    }
                    break;
                case "/rules":

                    BotDatabase.db.SaveChanges();
                    botClient.SendTextMessageAsync(message.Chat, $"Правила чата:\n{chat.Rules}");
                    break;
                case "/setrules":

                    BotDatabase.db.SaveChanges();
                    if (user.UserRights < UserRights.helper)
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.NextChatRules);
                        break;
                    }
                    break;
                case "/admin":
                    if (user.UserRights < UserRights.administrator)
                    {

                        if (replUser != null)
                        {
                            BotDatabase.db.SaveChanges();
                            if (user.UserRights < replUser.UserRights)
                            {
                                replUser.UserRights = UserRights.administrator;
                                BotDatabase.db.SaveChanges();
                                botClient.SendTextMessageAsync(message.Chat, BotPhrases.setRights + "администратор");
                                break;
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights);
                                break;
                            }
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, BotPhrases.ReplyToMesage);
                            break;
                        }
                    }
                    break;
                case "/moder":
                    if (user.UserRights < UserRights.moderator)
                    {

                        if (replUser != null)
                        {
                            if (user.UserRights < replUser.UserRights)
                            {
                                replUser.UserRights = UserRights.moderator;
                                BotDatabase.db.SaveChanges();
                                botClient.SendTextMessageAsync(message.Chat, BotPhrases.setRights + "модератор");
                                break;
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights);
                                break;
                            }
                        }

                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, BotPhrases.ReplyToMesage);
                            break;
                        }
                    }
                    break;
                case "/helper":
                    if (user.UserRights < UserRights.helper)
                    {
                        if (replUser != null)
                        {
                            if (user.UserRights < replUser.UserRights)
                            {
                                replUser.UserRights = UserRights.helper;
                                BotDatabase.db.SaveChanges();
                                botClient.SendTextMessageAsync(message.Chat, BotPhrases.setRights + "помощник");
                                break;
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights);
                                break;
                            }
                        }

                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, BotPhrases.ReplyToMesage);
                            break;
                        }
                    }
                    break;
                case "/normal":
                    if (user.UserRights < UserRights.normal)
                    {
                        if (replUser != null)
                        {
                            if (user.UserRights < replUser.UserRights)
                            {
                                replUser.UserRights = UserRights.normal;
                                BotDatabase.db.SaveChanges();
                                botClient.SendTextMessageAsync(message.Chat, BotPhrases.setRights[0] + "нормал");
                                break;
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights);
                                break;
                            }
                        }

                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, BotPhrases.ReplyToMesage);
                            break;
                        }
                    }
                    break;
                case "/mute":
                    if (user.UserRights < UserRights.helper)
                    {
                        if (replUser != null)
                        {
                            if (user.UserRights < replUser.UserRights)
                            {
                                if (replUser.IsMuted)
                                {
                                    replUser.IsMuted = false;
                                    BotDatabase.db.SaveChanges();
                                    replUser.Unmute();
                                    botClient.SendTextMessageAsync(message.Chat, $"Хорошо, по просьбе [{user.NicknameMd()}](tg://user?id={user.TelegramUserId}) я разрешаю [{replUser.NicknameMd()}](tg://user?id={replUser.TelegramUserId}) говорить", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                                    break;
                                }
                                else
                                {
                                    if (message.Text.Split().Length == 2)
                                    {
                                        try
                                        {
                                            int time = Convert.ToInt32(message.Text.Split()[1]);
                                            if (time > 0 && time < 99999)
                                            {
                                                replUser.IsMuted = true;
                                                BotDatabase.db.SaveChanges();
                                                replUser.Mute(time);
                                                botClient.SendTextMessageAsync(message.Chat, $"Ммм,  [{user.NicknameMd()}](tg://user?id={user.TelegramUserId}), какой ты строгий администратор... Но хорошо, ладно, только ради тебя, запрещаю [{replUser.NicknameMd()}](tg://user?id={replUser.TelegramUserId}) говорить", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                                                break;
                                            }
                                            else
                                            {
                                                botClient.SendTextMessageAsync(message.Chat, "Эй! Это же почти вечность! Давай выберем другое время, допустим от 0 до 99999", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                                                break;
                                            }
                                        }
                                        catch
                                        {
                                            botClient.SendTextMessageAsync(message.Chat, "Так, окей, а на сколько минут запретить то?", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        botClient.SendTextMessageAsync(message.Chat, "Так, окей, а на сколько минут запретить то?", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                                        break;
                                    }
                                }
                            }

                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, BotPhrases.ReplyToMesage);
                            break;
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                        break;
                    }
                    break;
                case "/stat":
                    if (user.UserRights < UserRights.helper)
                    {
                        if (replUser != null)
                        {
                            if (user.UserRights < replUser.UserRights)
                            {
                                botClient.SendTextMessageAsync(message.Chat, replUser.GetInfo(), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                                break;
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights);
                                break;
                            }

                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, user.GetInfo(), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            break;
                        }
                    }
                    break;
                case "/setwarninglimitaction":
                    botClient.SendTextMessageAsync(message.Chat, chat.SetWarningLimitAction(message.From.Id, message.Text));
                    break;
                case "/muted":
                    if (user.UserRights < UserRights.helper)
                    {
                        botClient.SendTextMessageAsync(message.Chat, chat.GetMutedUsers(), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                        break;
                    }
                    break;
                case "/ban":
                    if (replUser != null)
                    {
                        if (user.UserRights < UserRights.moderator && user.UserRights < replUser.UserRights)
                        {
                            replUser.Ban();
                            botClient.SendTextMessageAsync(message.Chat, $"Ну ты, [{user.NicknameMd()}](tg://user?id={user.TelegramUserId}), прям сама строгость! Прощай [{replUser.NicknameMd()}](tg://user?id={replUser.TelegramUserId}) " +
                                $"если передумаешь то напиши /unban в ответ на любое его сообщение, а затем пригласи его в чат " +
                                $"или вручную удалить его из черного списка чата, а затем пригласи ", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            break;
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights,
                            Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            break;
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.ReplyToMesage,
                            Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                        break;
                    }
                case "/unban":
                    if (replUser != null)
                    {
                        if (user.UserRights < UserRights.moderator && user.UserRights < replUser.UserRights)
                        {

                            replUser.UnBan();
                            botClient.SendTextMessageAsync(message.Chat, $"[{user.NicknameMd()}](tg://user?id={user.TelegramUserId}), ты милашка! Прощаем " +
                                $"[{replUser.NicknameMd()}](tg://user?id={replUser.TelegramUserId})! Теперь он снова может быть приглашен в чат",
                                Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            break;
                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights,
                                Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            break;
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.ReplyToMesage,
                            Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                        break;
                    }
                case "/warn":
                    if (replUser != null)
                    {
                        if (user.UserRights < UserRights.helper && user.UserRights < replUser.UserRights)
                        {
                            //TODO переделать. Будет отображаться неверное число варнов
                            if (replUser.WarnsCount != chat.WarnsLimit - 1)
                            {
                                replUser.Warn();
                                if (message.Text.Length > 6)
                                {
                                    botClient.SendTextMessageAsync(message.Chat, $"[{user.NicknameMd()}](tg://user?id={user.TelegramUserId}) предупреждает тебя, [{user.NicknameMd()}](tg://user?id={user.TelegramUserId})\nза {message.Text.Substring(5)}\nПредупреждений до наказания {chat.WarnsLimit - replUser.WarnsCount}", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                                    break;
                                }
                                else
                                {
                                    botClient.SendTextMessageAsync(message.Chat, $"[{user.NicknameMd()}](tg://user?id={user.TelegramUserId}) предупреждает тебя, [{replUser.NicknameMd()}](tg://user?id={replUser.TelegramUserId})\nПредупреждений до наказания {chat.WarnsLimit - replUser.WarnsCount}", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                                    break;
                                }
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, replUser.WarningLimitAction());
                            }

                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            break;
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.ReplyToMesage, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                        break;
                    }
                    break;
                case "/unwarn":
                    if (replUser != null)
                    {
                        if (user.UserRights < UserRights.helper && user.UserRights < replUser.UserRights)
                        {
                            replUser.WarnsCount = 0;
                            BotDatabase.db.SaveChanges();
                            botClient.SendTextMessageAsync(message.Chat, $"[{user.NicknameMd()}](tg://user?id={user.TelegramUserId}) прощает " +
                                $"[{replUser.NicknameMd()}](tg://user?id={replUser.TelegramUserId})",
                                Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            break;

                        }
                        else
                        {
                            botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights,
                                Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            break;
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.ReplyToMesage,
                            Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                        break;
                    }
                case "/warns":
                    if (user.UserRights < UserRights.helper)
                    {
                        botClient.SendTextMessageAsync(message.Chat, chat.GetWarnedUsers(),
                            Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                        break;
                    }
                    break;
                case "/voicemessage":
                    if (user.UserRights < UserRights.moderator)
                    {
                        if (!chat.VoiceMessagesDisallowed)
                        {
                            chat.VoiceMessagesDisallowed = true;
                            botClient.SendTextMessageAsync(message.Chat, "Ура! теперь без гс!");
                        }
                        else
                        {
                            chat.VoiceMessagesDisallowed = false;
                            botClient.SendTextMessageAsync(message.Chat, "Ладно уж, кидайте свои кружочки... Даже гс можно(");
                        }
                        break;
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                        break;
                    }
                case "/nick":
                    if (message.Text.Length > 8)
                    {
                        if (message.Text.Length < 30)
                        {
                            try
                            {
                                string nick = message.Text;
                                if (message.Entities.Count(s => s.Type == Telegram.Bot.Types.Enums.MessageEntityType.Code) > 0)
                                {
                                    List<Telegram.Bot.Types.MessageEntity> codes = message.Entities.Where(e => e.Type == Telegram.Bot.Types.Enums.MessageEntityType.Code).ToList();
                                    foreach (MessageEntity messageEntity in codes)
                                    {
                                        nick = $"{nick.Substring(0, messageEntity.Offset)}`{nick.Substring(messageEntity.Offset, messageEntity.Offset + messageEntity.Length)}`{nick.Substring(messageEntity.Offset + messageEntity.Length)}";
                                    }
                                }
                                user.Nickname = message.Text.Substring(6);
                                BotDatabase.db.SaveChanges();
                                botClient.SendTextMessageAsync(message.Chat, $"Окей, теперь ты [{user.NicknameMd()}](tg://user?id={user.TelegramUserId})", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            }
                            catch
                            {
                                botClient.SendTextMessageAsync(message.Chat, $"Кажется какие-то символы были недопустимы для ника", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            }
                        }
                        else
                        {
                            await botClient.SendTextMessageAsync(message.Chat, $"Разве это ник? Белиберда какая-то! Слишком длинно, я не запомню!", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                        }
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, $"Коротко не всегда хорошо, давай что-нибудь другое", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                    }
                    break;
                case "/nicks":
                    botClient.SendTextMessageAsync(message.Chat, user.Chat.GetChatNicknames(), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                    break;
                case "/rnd":
                    if (message.Text.Length > 6)
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotGames.GetRandomNumber(message.Text), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                    }
                    break;
                case "/chs":
                    if (message.Text.Length > 11)
                    {
                        user.LastMessage = message.Text;
                        BotDatabase.db.SaveChanges();
                        botClient.SendTextMessageAsync(message.Chat, BotGames.Chose(message.Text), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.ShortMessage, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                    }
                    break;
                case "/me":
                    if (message.Text.Length > 4)
                    {
                        if (message.ReplyToMessage != null)
                        {
                            string mestext = message.Text.Substring(4);
                            botClient.SendTextMessageAsync(message.Chat, $"[{user.NicknameMd()}](tg://user?id={user.TelegramUserId}) " + mestext + $" [{replUser.NicknameMd()}](tg://user?id={replUser.TelegramUserId})", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);

                        }
                        else
                        {
                            string mestext = message.Text.Substring(4);
                            botClient.SendTextMessageAsync(message.Chat, $"[{user.NicknameMd()}](tg://user?id={user.TelegramUserId}) " + mestext, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            botClient.DeleteMessageAsync(message.Chat.Id, message.MessageId);
                        }
                    }
                    break;
                case "/wh":
                    if (message.Text.Length > 4)
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotGames.Who(message.Text, user.Chat), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.ShortMessage, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                    }
                    break;
                case "/prob":
                    if (message.Text.Length > 5)
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotGames.Probability(message.Text), Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                    }
                    else
                    {
                        botClient.SendTextMessageAsync(message.Chat, BotPhrases.ShortMessage, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                    }
                    break;
                default:
                    switch (user.LastMessage)
                    {
                        case "/setrules":
                            if (user.UserRights < UserRights.moderator)
                            {
                                if (message.Text.Length > 15)
                                {
                                    if (message.Text.Length < 10000)
                                    {

                                        string rules = message.Text;
                                        chat.Rules = rules;
                                        botClient.SendTextMessageAsync(message.Chat, $"Теперь у нас есть законы", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                                    }
                                    else
                                    {
                                        botClient.SendTextMessageAsync(message.Chat, "Как-то слишком длинно, я не запомню...", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                                    }
                                }
                                else
                                {
                                    botClient.SendTextMessageAsync(message.Chat, "Разве же это правила? Так пару символов просто, давай что-нибудь подробнее!", Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                                }
                            }
                            else
                            {
                                botClient.SendTextMessageAsync(message.Chat, BotPhrases.NotEnoughtRights, Telegram.Bot.Types.Enums.ParseMode.MarkdownV2);
                            }
                            break;
                        default:
                            break;
                    }
                    break;

            }

            user.UpdateStatistic(message);
        }
    }
}
