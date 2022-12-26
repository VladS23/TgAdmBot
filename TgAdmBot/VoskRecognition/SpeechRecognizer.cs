using System.Diagnostics;
using Telegram.Bot;
using TgAdmBot.BotSpace;
using TgAdmBot.Database;
using TgAdmBot.Logger;
using Vosk;

namespace TgAdmBot.VoskRecognition
{
    internal class SpeechRecognizer
    {
        public static VoskRecognizer voskRecognizer = new VoskRecognizer(new Model("vosk-model"), 48000.0f);

        private static Queue<VoiceRecognitionObject> VoiceRecognitionObjects = new Queue<VoiceRecognitionObject>();
        private static Thread voiceRecognitionThread = new Thread(StartVoiceRecognition);

        private static Queue<VideoNoteRecognitionObject> VideoNoteRecognitionObjects = new Queue<VideoNoteRecognitionObject>();
        private static Thread videoNoteRecognitionThread = new Thread(StartVideoNoteRecognition);
        public static void AddVoiceMessageToQueue(VoiceRecognitionObject recognitionObject)
        {
            VoiceRecognitionObjects.Enqueue(recognitionObject);
            if (VoiceRecognitionObjects.Count < 2
                && (voiceRecognitionThread.ThreadState == System.Threading.ThreadState.Unstarted
                    || voiceRecognitionThread.ThreadState != System.Threading.ThreadState.Aborted)
                )
            {
                voiceRecognitionThread = new Thread(StartVoiceRecognition);
                voiceRecognitionThread.Start();
            }
        }

        public static void AddVideoNoteMessageToQueue(VideoNoteRecognitionObject recognitionObject)
        {
            VideoNoteRecognitionObjects.Enqueue(recognitionObject);
            if (VideoNoteRecognitionObjects.Count < 2
                && (videoNoteRecognitionThread.ThreadState == System.Threading.ThreadState.Unstarted
                    || videoNoteRecognitionThread.ThreadState != System.Threading.ThreadState.Aborted)
                )
            {
                videoNoteRecognitionThread = new Thread(StartVideoNoteRecognition);
                videoNoteRecognitionThread.Start();
            }
        }

        private static void RecognizeFileSpeech(string fileLocation, Database.Chat chat, int messageId, VoiceRecognitionObject recognitionObject)
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
            new Log($"RecognizeFileSpeech\n{result.text}");

            void WriteToDb()
            {
                VoiceMessage voice = BotDatabase.db.VoiceMessages.Single(vm => vm.Chat.ChatId == chat.ChatId && vm.MessageId == recognitionObject.voiceMessage.MessageId && vm.fileUniqueId == recognitionObject.voiceMessage.Voice.FileUniqueId);
                voice.recognizedText = result.text;
                BotDatabase.db.SaveChanges();
            }

            try
            {
                WriteToDb();
            }
            catch (System.InvalidOperationException)
            {
                Thread.Sleep(200);
                WriteToDb();
            }

        }

        private static void RecognizeFileSpeech(string fileLocation, Database.Chat chat, int messageId, VideoNoteRecognitionObject recognitionObject)
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
            new Log($"RecognizeFileSpeech(video_note)\n{result.text}");

            void WriteToDb()
            {
                VoiceMessage voice = BotDatabase.db.VoiceMessages.Single(vm => vm.Chat.ChatId == chat.ChatId && vm.MessageId == messageId && vm.fileUniqueId == recognitionObject.videoNoteMessage.VideoNote.FileUniqueId);
                voice.recognizedText = result.text;
                BotDatabase.db.SaveChanges();
            }

            try
            {
                WriteToDb();
            }
            catch (System.InvalidOperationException)
            {
                Thread.Sleep(200);
                WriteToDb();
            }

        }

        private static async void StartVoiceRecognition()
        {
            while (VoiceRecognitionObjects.Count > 0)
            {
                VoiceRecognitionObject recognitionObject = VoiceRecognitionObjects.Peek();

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

                try
                {
                    Stream destStream = System.IO.File.OpenWrite($"{fileLocation}.ogg");
                    await Bot.currentObject.GetInfoAndDownloadFileAsync(recognitionObject.voiceMessage.Voice.FileId, destStream);
                    destStream.Close();
                    destStream.Dispose();

                    Process convert = Process.Start("ffmpeg.exe", $"-i {fileLocation}.ogg {fileLocation}.wav");
                    convert.WaitForExit();
                    System.IO.File.Delete($"{fileLocation}.ogg");
                    RecognizeFileSpeech($"{fileLocation}.wav", recognitionObject.chat, recognitionObject.voiceMessage.MessageId, recognitionObject);

                    VoiceRecognitionObjects.Dequeue();
                }
                catch(Exception e)
                {
                    new Log($"Recognition error:\n{e.ToString()}", LogType.error);
                    continue;
                }
                
            }
            voiceRecognitionThread.Interrupt();
        }




        private static async void StartVideoNoteRecognition()
        {
            while (VideoNoteRecognitionObjects.Count > 0)
            {
                VideoNoteRecognitionObject recognitionObject = VideoNoteRecognitionObjects.Peek();

                string fileName = $"{recognitionObject.videoNoteMessage.From.Id}_{recognitionObject.videoNoteMessage.Chat.Id}_{recognitionObject.videoNoteMessage.MessageId}";
                if (!Directory.Exists("videonotes"))
                {
                    Directory.CreateDirectory("videonotes");
                }
                string fileLocation = $"{Directory.GetCurrentDirectory()}\\videonotes\\{fileName}";

                if (System.IO.File.Exists($"{fileLocation}.mp4"))
                {
                    System.IO.File.Delete($"{fileLocation}.mp4");
                }

                if (System.IO.File.Exists($"{fileLocation}.wav"))
                {
                    System.IO.File.Delete($"{fileLocation}.wav");
                }

                Stream destStream = System.IO.File.OpenWrite($"{fileLocation}.mp4");
                Telegram.Bot.Types.File file = await Bot.currentObject.GetInfoAndDownloadFileAsync(recognitionObject.videoNoteMessage.VideoNote.FileId, destStream);
                destStream.Close();
                destStream.Dispose();
                bool fileExists = System.IO.File.Exists($"{fileLocation}.mp4");
                Process convert = Process.Start($"ffmpeg.exe", $"-i {fileLocation}.mp4 -vn -acodec pcm_s16le -ar 44100 -ac 2 {fileLocation}.wav");
                convert.WaitForExit();
                System.IO.File.Delete($"{fileLocation}.mp4");
                new Log($"{fileLocation}.wav");
                RecognizeFileSpeech($"{fileLocation}.wav", recognitionObject.chat, recognitionObject.videoNoteMessage.MessageId, recognitionObject);

                VideoNoteRecognitionObjects.Dequeue();
            }
        }
    }
}
