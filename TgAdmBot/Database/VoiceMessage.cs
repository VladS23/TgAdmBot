using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TgAdmBot.Database
{
    public class VoiceMessage
    {
        public int VoiceMessageId { get; set; }
        public Chat Chat { get; set; }  
        public int MessageId { get; set; }
        public string fileId { get; set; }
        public string fileUniqueId { get; set; }
        public string recognizedText { get; set; } = "Текст ещё распознаётся, попробуйте выполнить команду позже.";

    }
}
