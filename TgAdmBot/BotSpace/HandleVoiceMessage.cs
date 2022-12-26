using Telegram.Bot;
using TgAdmBot.Database;
using TgAdmBot.VoskRecognition;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {
        private async void HandleVoiceMessage(Telegram.Bot.Types.Message message, Database.User user, Database.Chat chat)
        {
            if (chat.VoiceMessagesDisallowed)
            {
                botClient.DeleteMessageAsync(message.Chat, message.MessageId);
            }
            else
            {
                BotDatabase.db.VoiceMessages.Add(new VoiceMessage { Chat = chat, MessageId = message.MessageId, fileId = message.Voice.FileId, fileUniqueId = message.Voice.FileUniqueId });
                BotDatabase.db.SaveChanges();
                //Поток обработки аудио
                SpeechRecognizer.AddVoiceMessageToQueue(new VoiceRecognitionObject { chat = chat, voiceMessage = message });
            }

            user.UpdateStatistic(message);
        }

    }

}

