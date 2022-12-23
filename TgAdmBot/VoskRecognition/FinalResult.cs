namespace TgAdmBot.VoskRecognition
{
    public class WordInfo
    {
        public float conf;
        public float end;
        public float start;
        public string word;

    }
    public class FinalResult
    {
        public WordInfo[] result;
        public string text;
    }
}
