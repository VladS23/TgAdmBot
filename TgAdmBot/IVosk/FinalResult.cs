using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgAdmBot.IVosk
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
