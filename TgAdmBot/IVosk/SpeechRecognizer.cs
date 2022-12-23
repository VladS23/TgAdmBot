using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TgAdmBot.BotSpace;
using TgAdmBot.Database;
using Vosk;

namespace TgAdmBot.IVosk
{
    internal class SpeechRecognizer
    {
        public static VoskRecognizer voskRecognizer = new VoskRecognizer(new Model("vosk-model-ru-0.22"), 48000.0f);

        private static Queue<RecognitionObject> RecognitionObjects = new Queue<RecognitionObject>();
        private static Thread recognitionThread = new Thread(StartRecognition);
        public static void AddMessageToQueue(RecognitionObject recognitionObject)
        {
            RecognitionObjects.Enqueue(recognitionObject);
            if (RecognitionObjects.Count < 2
                && (recognitionThread.ThreadState == System.Threading.ThreadState.Unstarted
                    || recognitionThread.ThreadState != System.Threading.ThreadState.Aborted)
                )
            {
                recognitionThread = new Thread(StartRecognition);
                recognitionThread.Start();
            }
        }

        private static async void StartRecognition()
        {
            while (RecognitionObjects.Count > 0)
            {
                RecognitionObject recognitionObject = RecognitionObjects.Peek();

                void RecognizeFileSpeech(string fileLocation, Database.Chat chat, int messageId)
                {


                    using (Stream source = System.IO.File.OpenRead(fileLocation))
                    {
                        byte[] buffer = new byte[source.Length];
                        int bytesRead;
                        while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            voskRecognizer.AcceptWaveform(buffer, bytesRead);
                        }
                    }
                    FinalResult result = Newtonsoft.Json.JsonConvert.DeserializeObject<FinalResult>(voskRecognizer.FinalResult());
                    Console.WriteLine(result.text);
                    BotDatabase.db.VoiceMessages.Single(vm => vm.Chat.ChatId == chat.ChatId && vm.MessageId == recognitionObject.voiceMessage.MessageId&&vm.fileUniqueId==recognitionObject.voiceMessage.Voice.FileUniqueId).recognizedText = result.text;
                    BotDatabase.db.SaveChanges();
                }





                string fileName = $"{recognitionObject.voiceMessage.From.Id}_{recognitionObject.voiceMessage.Chat.Id}_{recognitionObject.voiceMessage.MessageId}";
                if (!Directory.Exists("voicemessages"))
                {
                    Directory.CreateDirectory("voicemessages");
                }
                string fileLocation = $"{Directory.GetCurrentDirectory()}\\voicemessages\\{fileName}";
                if (System.IO.File.Exists($"{fileLocation}.ogg"))
                {
                    System.IO.File.Delete($"{fileLocation}.ogg");
                }

                if (System.IO.File.Exists($"{fileLocation}.wav"))
                {
                    System.IO.File.Delete($"{fileLocation}.wav");
                }

                Stream destStream = System.IO.File.OpenWrite($"{fileLocation}.ogg");
                await Bot.currentObject.GetInfoAndDownloadFileAsync(recognitionObject.voiceMessage.Voice.FileId, destStream);
                destStream.Close();
                destStream.Dispose();

                Process convert = Process.Start("ffmpeg.exe", $"-i {fileLocation}.ogg {fileLocation}.wav");
                convert.WaitForExit();
                System.IO.File.Delete($"{fileLocation}.ogg");
                Console.WriteLine($"{fileLocation}.wav");
                RecognizeFileSpeech($"{fileLocation}.wav", recognitionObject.chat, recognitionObject.voiceMessage.MessageId);

                RecognitionObjects.Dequeue();
            }
            recognitionThread.Interrupt();
        }
    }
}
