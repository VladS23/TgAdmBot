using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using TgAdmBot.Database;

namespace TgAdmBot.BotSpace
{
    internal partial class Bot
    {
        private async Task HandleVoiceMessage(CancellationToken cancellationToken, Telegram.Bot.Types.Message message, Database.User user, Database.Chat chat)
        {
            Task recognitionTask = Task.Run(() =>
            {
                if (chat.VoiceMessagesDisallowed)
                {
                    botClient.DeleteMessageAsync(message.Chat, message.MessageId);
                }
                else
                {

                    // Create an in-process speech recognizer for the en-US locale.  
                    using (
                    SpeechRecognitionEngine recognizer =
                      new SpeechRecognitionEngine(
                        new System.Globalization.CultureInfo("ru-RU")))
                    {

                        // Create and load a dictation grammar.  
                        recognizer.LoadGrammar(new DictationGrammar());

                        // Add a handler for the speech recognized event.  
                        recognizer.SpeechRecognized +=
                          new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

                        // Configure input to the speech recognizer.  
                        recognizer.SetInputToWaveFile("");

                        // Start asynchronous, continuous speech recognition.  
                        recognizer.RecognizeAsync(RecognizeMode.Single);

                        void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
                        {
                            Console.WriteLine("Recognized text: " + e.Result.Text);
                        }

                    }
                }
            });

            user.UpdateStatistic(message);
        }
    }

}

