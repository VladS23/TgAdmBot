using TgAdmBot.Database;
using TgAdmBot.VoskRecognition;

namespace TgAdmBot.BotSpace
{

    internal partial class Bot
    {
        private async void HandleVideoNoteMessage(Telegram.Bot.Types.Message message, Database.User user, Database.Chat chat)
        {
            BotDatabase.db.VoiceMessages.Add(new VoiceMessage { Chat = chat, MessageId = message.MessageId, fileId = message.VideoNote.FileId, fileUniqueId = message.VideoNote.FileUniqueId });
            BotDatabase.db.SaveChanges();
            SpeechRecognizer.AddVideoNoteMessageToQueue(new VideoNoteRecognitionObject { chat = chat, videoNoteMessage = message });
        }
    }
}
